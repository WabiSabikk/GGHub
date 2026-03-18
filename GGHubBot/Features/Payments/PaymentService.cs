using GGHubBot.Services;
using GGHubShared.Enums;
using GGHubShared.Models;

namespace GGHubBot.Features.Payments
{
    public class PaymentService
    {
        private readonly ApiService _apiService;

        public PaymentService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<ApiResponse<DuelDto>?> ProcessDuelPaymentAsync(Guid duelId, Guid userId, PaymentProvider provider)
        {
            return await _apiService.PayEntryFeeAsync(duelId, userId, provider);
        }

        public async Task<CryptomusPaymentResult?> CheckPaymentStatusAsync(string paymentId)
        {
            return await _apiService.CheckPaymentAsync(paymentId);
        }

        public static string ExtractPaymentId(string url)
        {
            try
            {
                var uri = new Uri(url);
                var segment = uri.Segments.Last().Trim('/');
                return segment;
            }
            catch
            {
                return url;
            }
        }
    }
}
