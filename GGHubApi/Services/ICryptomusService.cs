using GGHubShared.Models;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace GGHubApi.Services
{
    public interface ICryptomusService
    {
        Task<CryptomusPaymentResult> CreatePaymentAsync(CreateCryptomusPaymentRequest request);
        Task<CryptomusPaymentResult> CheckPaymentAsync(string paymentId);
        Task<CryptomusBalanceResult> GetBalanceAsync();
        bool ValidateWebhook(string payload, string signature);
        string GenerateSignature(string payload);
        Task<bool> SendTestWebhookAsync(string callbackUrl, string status = "paid", string? existingPaymentId = null);
    }

    public class CryptomusService : ICryptomusService
    {
        private readonly HttpClient _httpClient;
        private readonly string _merchantId;
        private readonly string _paymentKey;
        private readonly ILogger<CryptomusService> _logger;
#if DEBUG
        private const bool UseTestEnv = true;
#else
        private const bool UseTestEnv = false;
#endif

        private const string BaseUrl = "https://api.cryptomus.com/v1/";

        public CryptomusService(IConfiguration configuration, HttpClient httpClient, ILogger<CryptomusService> logger)
        {
            _merchantId = configuration["Cryptomus:MerchantId"] ?? throw new InvalidOperationException("Cryptomus MerchantId not configured");
            _paymentKey = configuration["Cryptomus:PaymentKey"] ?? throw new InvalidOperationException("Cryptomus PaymentKey not configured");
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _logger = logger;
        }

        public string GenerateSignature(string payload)
        {
            var payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
            var signatureInput = payloadBase64 + _paymentKey;

            using var md5 = MD5.Create();
            var inputBytes = Encoding.UTF8.GetBytes(signatureInput);
            var hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        public async Task<CryptomusPaymentResult> CreatePaymentAsync(CreateCryptomusPaymentRequest payment)
        {
            try
            {
                var payloadObj = new
                {
                    amount = payment.Amount.ToString(CultureInfo.InvariantCulture),
                    currency = "USD",
                    order_id = payment.OrderId,
                    url_return = payment.ReturnUrl,
                    url_callback = payment.CallbackUrl,
                    is_payment_multiple = false,
                    lifetime = 7200
                };

                var jsonPayload = JsonSerializer.Serialize(payloadObj, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var signature = GenerateSignature(jsonPayload);

                var endpoint = "payment";
                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Headers.Add("merchant", _merchantId);
                request.Headers.Add("sign", signature);
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Cryptomus payment created: {OrderId}, Status: {StatusCode}", payment.OrderId, response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Cryptomus payment creation failed: {Error}", responseContent);
                    return new CryptomusPaymentResult
                    {
                        Success = false,
                        Message = $"API Error: {responseContent}"
                    };
                }

                var responseObj = JsonSerializer.Deserialize<CryptomusPaymentApiResponse>(responseContent);
                return responseObj != null
                    ? CryptomusPaymentResult.FromApiResponse(responseObj)
                    : new CryptomusPaymentResult { Success = false, Message = "Invalid response format" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Cryptomus payment for order: {OrderId}", payment.OrderId);
                return new CryptomusPaymentResult
                {
                    Success = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        public async Task<CryptomusBalanceResult> GetBalanceAsync()
        {
            try
            {
                var payload = "{}";
                var signature = GenerateSignature(payload);

                var endpoint =  "balance";
                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Headers.Add("merchant", _merchantId);
                request.Headers.Add("sign", signature);
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Cryptomus balance check: {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Cryptomus balance check failed: {Error}", content);
                    return new CryptomusBalanceResult
                    {
                        Success = false,
                        Message = $"API Error ({response.StatusCode}): {content}"
                    };
                }

                var apiResponse = JsonSerializer.Deserialize<CryptomusApiResponse>(content);
                return apiResponse != null ? CryptomusBalanceResult.FromApiResponse(apiResponse) :
                    new CryptomusBalanceResult { Success = false, Message = "Failed to parse API response" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Cryptomus balance");
                return new CryptomusBalanceResult
                {
                    Success = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        public async Task<CryptomusPaymentResult> CheckPaymentAsync(string paymentId)
        {
            try
            {
                var payload = JsonSerializer.Serialize(new { uuid = paymentId });
                var signature = GenerateSignature(payload);

                var endpoint = "payment/info";
                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Headers.Add("merchant", _merchantId);
                request.Headers.Add("sign", signature);
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Cryptomus payment check: {PaymentId}, Status: {StatusCode}", paymentId, response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Cryptomus payment check failed: {Error}", content);
                    return new CryptomusPaymentResult { Success = false, Message = $"API Error: {content}" };
                }

                var responseObj = JsonSerializer.Deserialize<CryptomusPaymentApiResponse>(content);
                return responseObj != null
                    ? CryptomusPaymentResult.FromApiResponse(responseObj)
                    : new CryptomusPaymentResult { Success = false, Message = "Invalid response format" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Cryptomus payment: {PaymentId}", paymentId);
                return new CryptomusPaymentResult { Success = false, Message = $"Exception: {ex.Message}" };
            }
        }

        public bool ValidateWebhook(string payload, string signature)
        {
            try
            {
                // 1. Десеріалізуємо payload в Dictionary
                var webhookData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payload, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (webhookData == null)
                {
                    _logger.LogWarning("Failed to deserialize webhook payload for validation");
                    return false;
                }

                // 2. ВИДАЛЯЄМО підпис з даних (як вимагає документація)
                webhookData.Remove("sign");

                // 3. Серіалізуємо назад БЕЗ підпису
                var dataWithoutSign = JsonSerializer.Serialize(webhookData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    // Важливо для сумісності з PHP
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                // 4. Кодуємо в base64 (згідно документації)
                var payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(dataWithoutSign));

                // 5. Додаємо API ключ
                var signatureInput = payloadBase64 + _paymentKey;

                // 6. Створюємо MD5 хеш
                using var md5 = MD5.Create();
                var inputBytes = Encoding.UTF8.GetBytes(signatureInput);
                var hashBytes = md5.ComputeHash(inputBytes);
                var calculatedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                // 7. Порівнюємо підписи
                var isValid = calculatedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);

                if (!isValid)
                {
                    _logger.LogWarning("Webhook signature validation failed");
                    _logger.LogDebug("Expected signature: {Expected}", calculatedSignature);
                    _logger.LogDebug("Received signature: {Received}", signature);
                    _logger.LogDebug("Data without sign: {Data}", dataWithoutSign);
                    _logger.LogDebug("Base64 payload: {Base64}", payloadBase64);
                }
                else
                {
                    _logger.LogInformation("Webhook signature validation successful");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating webhook signature");
                return false;
            }
        }

       
        public async Task<bool> SendTestWebhookAsync(string callbackUrl, string status = "paid", string? existingPaymentId = null)
        {
            try
            {
                // Параметри згідно документації
                var payloadObj = new
                {
                    url_callback = callbackUrl,
                    currency = "eth",      // обов'язковий
                    network = "eth",       // обов'язковий  
                    status = status,       // обов'язковий
                    uuid = existingPaymentId // опціональний
                };

                var jsonPayload = JsonSerializer.Serialize(payloadObj, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var signature = GenerateSignature(jsonPayload);

                // Endpoint згідно документації
                var endpoint = "test-webhook/payment"; // завжди тестовий для цього методу
                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Headers.Add("merchant", _merchantId);
                request.Headers.Add("sign", signature);
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Test webhook request sent to Cryptomus API for callback: {CallbackUrl}", callbackUrl);
                _logger.LogInformation("Cryptomus response - Status: {Status}, Content: {Content}",
                    response.StatusCode, responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to send test webhook: {Error}", responseContent);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test webhook request to Cryptomus API");
                return false;
            }
        }
    }

  
}