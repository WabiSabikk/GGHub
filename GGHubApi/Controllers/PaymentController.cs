using GGHubApi.Services;
using GGHubDb.Services;
using GGHubShared.Enums;
using GGHubShared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GGHubApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly ICryptomusService _cryptomusService;
    private readonly ITransactionService _transactionService;
    private readonly IDuelService _duelService;
    private readonly ILogger<PaymentController> _logger;
    private readonly string _publicBaseUrl;

    public PaymentController(
        ICryptomusService cryptomusService,
        ITransactionService transactionService,
        IDuelService duelService,
        IConfiguration configuration,
        ILogger<PaymentController> logger)
    {
        _cryptomusService = cryptomusService;
        _transactionService = transactionService;
        _duelService = duelService;
        _logger = logger;
        _publicBaseUrl = configuration["App:PublicBaseUrl"] ?? string.Empty;
    }

    [HttpPost("deposit")]
    public async Task<ActionResult<ApiResponse<CryptomusPaymentResult>>> CreateDeposit([FromBody] decimal amount, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return BadRequest(new ApiResponse<CryptomusPaymentResult>
            {
                Success = false,
                Code = ErrorCode.ValidationFailed
            });

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var txResult = await _transactionService.CreateDepositAsync(userId, amount, PaymentProvider.Cryptomus, null, cancellationToken);
        if (!txResult.Success || txResult.Data == null)
            return BadRequest(new ApiResponse<CryptomusPaymentResult>
            {
                Success = false,
                Code = txResult.Code
            });

        var paymentRequest = new CreateCryptomusPaymentRequest
        {
            Amount = amount,
            OrderId = $"deposit-{txResult.Data.Id}",
            ReturnUrl = $"{_publicBaseUrl}payment/success",
            CallbackUrl = $"{_publicBaseUrl}api/webhook/cryptomus"
        };

        var paymentResult = await _cryptomusService.CreatePaymentAsync(paymentRequest);
        if (!paymentResult.Success)
        {
            await _transactionService.FailTransactionAsync(txResult.Data.Id, paymentResult.Message ?? "Payment error", cancellationToken);
            return BadRequest(new ApiResponse<CryptomusPaymentResult>
            {
                Success = false,
                Code = ErrorCode.PaymentError
            });
        }

        var expiresAt = paymentResult.ExpiredAt.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(paymentResult.ExpiredAt.Value).UtcDateTime
            : (DateTime?)null;
        await _transactionService.UpdateTransactionInfoAsync(
            txResult.Data.Id,
            paymentResult.PaymentId,
            paymentResult.PaymentUrl,
            expiresAt,
            cancellationToken);

        return Ok(new ApiResponse<CryptomusPaymentResult> { Success = true, Data = paymentResult });
    }

    [HttpGet("check")]
    public async Task<ActionResult<CryptomusPaymentResult>> CheckPayment([FromQuery] string paymentId)
    {
        var result = await _cryptomusService.CheckPaymentAsync(paymentId);
        if (result == null)
            return BadRequest(new ApiResponse<CryptomusPaymentResult>
            {
                Success = false,
                Code = ErrorCode.NotFound
            });
        result.Success = true;
        return Ok(result);
    }

#if DEBUG
    [HttpPost("test-webhook")]
    [AllowAnonymous] 
    public async Task<ActionResult> TestWebhook([FromQuery] string status = "paid", [FromQuery] string? paymentId = null)
    {
        try
        {
            // URL вашого webhook endpoint'у
            var callbackUrl = $"{_publicBaseUrl}api/webhook/cryptomus";

            _logger.LogInformation("Triggering test webhook via Cryptomus API");
            _logger.LogInformation("Callback URL: {CallbackUrl}", callbackUrl);
            _logger.LogInformation("Status: {Status}", status);

            // ¬икликаЇмо оф≥ц≥йний Test Webhook API
            var result = await _cryptomusService.SendTestWebhookAsync(callbackUrl, status, paymentId);

            if (result)
            {
                return Ok(new
                {
                    success = true,
                    message = "Test webhook triggered successfully via Cryptomus API",
                    callback_url = callbackUrl,
                    status = status,
                    existing_payment_id = paymentId
                });
            }

            return BadRequest(new
            {
                success = false,
                message = "Failed to trigger test webhook via Cryptomus API. Check logs for details."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering test webhook");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }
#endif


    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<PagedResult<TransactionDto>>>> GetHistory([FromQuery] int page = 1, CancellationToken cancellationToken = default)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var result = await _transactionService.GetByUserIdAsync(userId, page, 20, cancellationToken: cancellationToken);
        return Ok(result);
    }
}
