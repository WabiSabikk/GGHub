using GGHubApi.Hubs;
using GGHubApi.Services;
using GGHubDb.Services;
using GGHubShared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GGHubApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class WebhookController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ITournamentService _tournamentService;
        private readonly IDuelService _duelService;
        private readonly IDuelHubService _duelHubService;
        private readonly IDathostWebhookService _dathostWebhookService;
        private readonly ICryptomusService _cryptomusService;
        private readonly ILogger<WebhookController> _logger;
        private readonly string? _cryptomusWebhookIP;

        public WebhookController(
            ITournamentService tournamentService,
            IDuelService duelService,
            IDathostWebhookService dathostWebhookService,
            ITransactionService transactionService,
            ICryptomusService cryptomusService,
            IDuelHubService duelHubService,
            ILogger<WebhookController> logger,
            IConfiguration configuration)
        {
            _tournamentService = tournamentService;
            _duelService = duelService;
            _transactionService = transactionService;
            _cryptomusService = cryptomusService;
            _duelHubService = duelHubService;
            _dathostWebhookService = dathostWebhookService;
            _logger = logger;
            _cryptomusWebhookIP = configuration["Cryptomus:WebhookIP"];
        }

        [HttpPost("dathost")]
        public async Task<IActionResult> HandleDathostWebhook([FromBody] JsonElement payload, CancellationToken cancellationToken = default)
        {
            try
            {
                string text = payload.GetRawText();

                // Перевіряємо чи це votekick webhook
                if (payload.TryGetProperty("kicked", out _) && payload.TryGetProperty("match_id", out _))
                {
                    var votekickPayload = JsonSerializer.Deserialize<DathostVotekickWebhook>(text);
                    if (votekickPayload != null)
                    {
                        var result = await _dathostWebhookService.ProcessVotekickEventAsync(votekickPayload, cancellationToken);
                        return result.Success
                            ? Ok(new { success = true, message = "Votekick processed" })
                            : BadRequest(new { success = false, message = result.Message });
                    }
                }
                // Match webhook (event-driven або legacy)
                else if (payload.TryGetProperty("id", out _) && payload.TryGetProperty("game_server_id", out _))
                {
                    var matchPayload = JsonSerializer.Deserialize<DathostMatchWebhook>(text);
                    if (matchPayload != null)
                    {
                        var result = await _dathostWebhookService.ProcessMatchWebhookAsync(matchPayload, cancellationToken);
                        return result.Success
                            ? Ok(new { success = true, message = "Match webhook processed" })
                            : BadRequest(new { success = false, message = result.Message });
                    }
                }

                _logger.LogWarning("Unknown DatHost webhook payload structure: {PayloadKeys}",
                    string.Join(", ", payload.EnumerateObject().Select(p => p.Name)));
                return BadRequest(new { success = false, message = "Unknown webhook type" });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize DatHost webhook payload");
                return BadRequest(new { success = false, message = "Invalid JSON payload" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing DatHost webhook: {Error}", ex.Message);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpPost("cryptomus")]
        [AllowAnonymous]
        public async Task<IActionResult> ProcessCryptomusWebhook(CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. IP ВАЛІДАЦІЯ (якщо налаштована в appsettings)
                if (!string.IsNullOrEmpty(_cryptomusWebhookIP))
                {
                    var clientIP = HttpContext.Connection.RemoteIpAddress?.ToString();

                    if (clientIP != _cryptomusWebhookIP)
                    {
                        _logger.LogWarning("Invalid IP for Cryptomus webhook. Client: {ClientIP}, Expected: {ExpectedIP}",
                            clientIP, _cryptomusWebhookIP);
                        return BadRequest(new { success = false, message = "Invalid IP address" });
                    }

                    _logger.LogInformation("IP validation passed for Cryptomus webhook: {IP}", clientIP);
                }

                // 2. ЧИТАННЯ PAYLOAD
                var body = await new StreamReader(Request.Body).ReadToEndAsync();

                if (string.IsNullOrEmpty(body))
                {
                    _logger.LogWarning("Empty body in Cryptomus webhook");
                    return BadRequest(new { success = false, message = "Empty body" });
                }

                // 3. ВИТЯГУЄМО ПІДПИС З PAYLOAD (НЕ З ЗАГОЛОВКІВ!)
                var webhookData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (webhookData == null || !webhookData.ContainsKey("sign"))
                {
                    _logger.LogWarning("Missing sign field in Cryptomus webhook payload");
                    return BadRequest(new { success = false, message = "Missing signature" });
                }

                var signature = webhookData["sign"].GetString();
                if (string.IsNullOrEmpty(signature))
                {
                    _logger.LogWarning("Empty signature in Cryptomus webhook");
                    return BadRequest(new { success = false, message = "Empty signature" });
                }

                // 4. ВАЛІДАЦІЯ ПІДПИСУ (правильно, згідно документації)
                if (!_cryptomusService.ValidateWebhook(body, signature))
                {
                    _logger.LogWarning("Invalid signature in Cryptomus webhook");
                    return BadRequest(new { success = false, message = "Invalid signature" });
                }

                // 5. ДЕСЕРІАЛІЗАЦІЯ PAYLOAD
                var payload = JsonSerializer.Deserialize<CryptomusWebhookPayload>(body, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                });

                if (payload == null)
                {
                    _logger.LogWarning("Failed to deserialize Cryptomus webhook payload");
                    return BadRequest(new { success = false, message = "Invalid payload" });
                }

                _logger.LogInformation("Received Cryptomus webhook: PaymentId={PaymentId}, Status={Status}, OrderId={OrderId}",
                    payload.Uuid, payload.Status, payload.OrderId);

                // 6. ОБРОБКА СТАТУСІВ ПЛАТЕЖУ
                if (payload.Status == "paid" || payload.Status == "paid_over")
                {
                    var txResult = await _transactionService.GetByExternalIdAsync(payload.Uuid, cancellationToken);
                    if (txResult.Success && txResult.Data != null)
                    {
                        await _transactionService.CompleteTransactionAsync(txResult.Data.Id, payload.Uuid, cancellationToken);

                        if (txResult.Data.DuelId.HasValue)
                        {
                            var duelResult = await _duelService.MarkEntryFeePaidAsync(
                                txResult.Data.DuelId.Value, txResult.Data.UserId, cancellationToken);
                            if (!duelResult.Success)
                            {
                                _logger.LogWarning("Failed to mark entry fee paid for duel: {DuelId}", txResult.Data.DuelId);
                            }

                            var participantUserIds = duelResult.Data?.Participants
                                .Select(p => p.User.Id)
                                .ToList() ?? new List<Guid>();

                            await _duelHubService.NotifyEntryFeePaid(
                                txResult.Data.DuelId.Value,
                                txResult.Data.UserId,
                                participantUserIds);
                        }

                        _logger.LogInformation("Successfully completed transaction: {TransactionId} for payment: {PaymentId}",
                            txResult.Data.Id, payload.Uuid);
                    }
                    else
                    {
                        _logger.LogWarning("Transaction not found for payment: {PaymentId}", payload.Uuid);
                        return BadRequest(new { success = false, message = "Transaction not found" });
                    }
                }
                else if (payload.Status == "cancel")
                {
                    var txResult = await _transactionService.GetByExternalIdAsync(payload.Uuid, cancellationToken);
                    if (txResult.Success && txResult.Data != null)
                    {
                        await _transactionService.CancelTransactionAsync(txResult.Data.Id, cancellationToken);
                        _logger.LogInformation("Successfully cancelled transaction: {TransactionId} for payment: {PaymentId}",
                            txResult.Data.Id, payload.Uuid);
                    }
                    else
                    {
                        _logger.LogWarning("Transaction not found for cancellation: {PaymentId}", payload.Uuid);
                    }
                }
                else
                {
                    _logger.LogInformation("Received Cryptomus webhook with unhandled status: {Status} for payment: {PaymentId}",
                        payload.Status, payload.Uuid);
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Cryptomus webhook");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }

    public class DathostWebhookPayload
    {
        public string Event { get; set; } = string.Empty;
        public string MatchId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Winner { get; set; } = string.Empty;
        public string CustomData { get; set; } = string.Empty;
        public DathostScore? Score { get; set; }
    }

    public class DathostScore
    {
        public int Team1 { get; set; }
        public int Team2 { get; set; }
    }

    public class CryptomusWebhookPayload
    {
        public string Uuid { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}