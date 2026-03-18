using GGHubShared.Models;
using GGHubShared.Enums;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Text;
using System.Text.Json;

namespace GGHubBot.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private string? _token;
        private DateTime _expiresAt;

        public ApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri(configuration["GGHubApi:BaseUrl"]!);

#if DEBUG
            _httpClient.Timeout = TimeSpan.FromSeconds(300);
#endif
        }

        public async Task InitializeAsync()
        {
            await AuthenticateAsync();
        }

        private async Task AuthenticateAsync()
        {
            var email = _configuration["BotAccount:Email"];
            var password = _configuration["BotAccount:PasswordHash"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return;

            var login = new LoginRequest { Email = email, PasswordHash = password };
            var json = JsonSerializer.Serialize(login, GetJsonOptions());
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("auth/login", content);
            if (response.IsSuccessStatusCode)
            {
                var respContent = await response.Content.ReadAsStringAsync();
                var auth = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(respContent, GetJsonOptions());
                if (auth?.Success == true && auth.Data != null)
                {
                    _token = auth.Data.Token;
                    _expiresAt = DateTime.UtcNow.AddMinutes(55);
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                }
            }
        }

        private async Task EnsureAuthenticatedAsync()
        {
            if (_token == null || DateTime.UtcNow >= _expiresAt)
            {
                await AuthenticateAsync();
            }
        }

        public async Task<ApiResponse<UserDto>?> GetUserByTelegramAsync(long telegramChatId)
        {
            await EnsureAuthenticatedAsync();
            try
            {
                var response = await _httpClient.GetAsync($"users/by-telegram/{telegramChatId}");
                return await ParseApiResponseAsync<UserDto>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting user by Telegram ID: {TelegramId}", telegramChatId);
                return null;
            }
        }

        public async Task<ApiResponse<UserDto>?> GetUserByIdAsync(Guid userId)
        {
            await EnsureAuthenticatedAsync();
            try
            {
                var response = await _httpClient.GetAsync($"users/{userId}");
                return await ParseApiResponseAsync<UserDto>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting user by ID: {UserId}", userId);
                return null;
            }
        }

        public async Task<ApiResponse<UserDto>?> CreateUserAsync(string username, string email, long telegramChatId, string? telegramUsername, string steamId)
        {
            await EnsureAuthenticatedAsync();
            try
            {
                var request = new CreateUserRequest
                {
                    Username = username,
                    Email = email
                };

                var json = JsonSerializer.Serialize(request, GetJsonOptions());
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("users", content);

                var userResponse = await ParseApiResponseAsync<UserDto>(response);

                if (userResponse?.Success == true && userResponse.Data != null)
                {
                    await LinkAccountsAsync(userResponse.Data.Id, steamId, telegramUsername, telegramChatId);
                }

                return userResponse;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating user");
                return null;
            }
        }

        public async Task<bool> LinkAccountsAsync(Guid userId, string steamId, string? telegramUsername, long telegramChatId)
        {
            await EnsureAuthenticatedAsync();
            try
            {
                var steamRequest = new LinkSteamRequest { SteamId = steamId };
                var steamJson = JsonSerializer.Serialize(steamRequest, GetJsonOptions());
                var steamContent = new StringContent(steamJson, Encoding.UTF8, "application/json");

                var steamResponse = await _httpClient.PostAsync($"users/{userId}/link-steam", steamContent);

                var telegramRequest = new LinkTelegramRequest
                {
                    TelegramUsername = telegramUsername ?? "",
                    TelegramChatId = telegramChatId
                };
                var telegramJson = JsonSerializer.Serialize(telegramRequest, GetJsonOptions());
                var telegramContent = new StringContent(telegramJson, Encoding.UTF8, "application/json");

                var telegramResponse = await _httpClient.PostAsync($"users/{userId}/link-telegram", telegramContent);

                return steamResponse.IsSuccessStatusCode && telegramResponse.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error linking accounts for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> LinkTelegramAccountAsync(Guid userId, string? telegramUsername, long telegramChatId)
        {
            await EnsureAuthenticatedAsync();
            try
            {
                var telegramRequest = new LinkTelegramRequest
                {
                    TelegramUsername = telegramUsername ?? string.Empty,
                    TelegramChatId = telegramChatId
                };
                var telegramJson = JsonSerializer.Serialize(telegramRequest, GetJsonOptions());
                var telegramContent = new StringContent(telegramJson, Encoding.UTF8, "application/json");

                var telegramResponse = await _httpClient.PostAsync($"users/{userId}/link-telegram", telegramContent);
                return telegramResponse.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error linking Telegram account for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<ApiResponse<List<DuelDto>>?> GetAvailableDuelsAsync()
        {
            await EnsureAuthenticatedAsync();
            try
            {
                var response = await _httpClient.GetAsync("duels/available");
                return await ParseApiResponseAsync<List<DuelDto>>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting available duels");
                return null;
            }
        }

        public async Task<ApiResponse<List<DuelDto>>?> GetUserDuelsAsync(Guid userId)
        {
            await EnsureAuthenticatedAsync();
            try
            {
                var response = await _httpClient.GetAsync($"duels/user/{userId}");
                return await ParseApiResponseAsync<List<DuelDto>>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting user duels for: {UserId}", userId);
                return null;
            }
        }

        public async Task<ApiResponse<DuelDto>?> GetFullDuelAsync(Guid duelId)
        {
            await EnsureAuthenticatedAsync();
            try
            {
                var response = await _httpClient.GetAsync($"duels/{duelId}/full");
                return await ParseApiResponseAsync<DuelDto>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting duel info: {DuelId}", duelId);
                return null;
            }
        }

        public async Task<ApiResponse<DuelDto>?> CreateDuelAsync(CreateDuelRequest request, Guid createdBy)
        {
            await EnsureAuthenticatedAsync();
            try
            {
                var json = JsonSerializer.Serialize(request, GetJsonOptions());
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"duels?createdBy={createdBy}", content);

                return await ParseApiResponseAsync<DuelDto>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating duel");
                return null;
            }
        }

        public async Task<ApiResponse<string>?> GenerateDuelInviteLinkAsync(Guid duelId)
        {
            await EnsureAuthenticatedAsync();
            try
            {
                var response = await _httpClient.PostAsync($"duels/{duelId}/invite-link", null);
                return await ParseApiResponseAsync<string>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating duel invite link");
                return null;
            }
        }

        public async Task<ApiResponse<DuelDto>?> JoinDuelAsync(Guid duelId, Guid userId)
        {
            await EnsureAuthenticatedAsync();
            try
            {
                var request = new JoinDuelRequest { DuelId = duelId };
                var json = JsonSerializer.Serialize(request, GetJsonOptions());
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"duels/{duelId}/join?userId={userId}", content);
                return await ParseApiResponseAsync<DuelDto>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error joining duel: {DuelId}", duelId);
                return null;
            }
        }

        public async Task<ApiResponse<DuelDto>?> JoinDuelByLinkAsync(string inviteLink, Guid userId)
        {
            await EnsureAuthenticatedAsync();
            try
            {
                var request = new JoinDuelByInviteLinkRequest { InviteLink = inviteLink };
                var json = JsonSerializer.Serialize(request, GetJsonOptions());
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"duels/join-by-link?userId={userId}", content);
                return await ParseApiResponseAsync<DuelDto>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error joining duel by link");
                return null;
            }
        }

        public async Task<ApiResponse<DuelDto>?> PayEntryFeeAsync(Guid duelId, Guid userId, PaymentProvider provider)
        {
            await EnsureAuthenticatedAsync();
            try
            {
                var response = await _httpClient.PostAsync($"duels/{duelId}/pay?userId={userId}&provider={provider}", null);
                return await ParseApiResponseAsync<DuelDto>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error paying entry fee for duel: {DuelId}", duelId);
                return null;
            }
        }

        public async Task<ApiResponse<OpponentStatus>> ConfirmReadyAsync(Guid duelId, Guid userId)
        {
            await EnsureAuthenticatedAsync();
            try
            {
                var response = await _httpClient.PostAsync($"duels/{duelId}/ready?userId={userId}", null);
                return await ParseApiResponseAsync<OpponentStatus>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error confirming ready for duel: {DuelId}", duelId);
                return null;
            }
        }

        public async Task<CryptomusPaymentResult?> CheckPaymentAsync(string paymentId)
        {
            try
            {
#if DEBUG
                var response = await _httpClient.PostAsync($"payment/test-webhook?status=paid&paymentId={paymentId}", null);
                return new CryptomusPaymentResult { Success = response.IsSuccessStatusCode, Status = "test" };
#else
                await EnsureAuthenticatedAsync();
                var response = await _httpClient.GetAsync($"payment/check?paymentId={paymentId}");
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    return null;
                return JsonSerializer.Deserialize<CryptomusPaymentResult>(content, GetJsonOptions());
#endif
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking payment: {PaymentId}", paymentId);
                return null;
            }
        }

        public async Task<ApiResponse<DuelDto>?> JoinDuelByCodeAsync(string inviteCode, Guid userId)
        {
            return await JoinDuelByLinkAsync(inviteCode, userId);
        }

        public async Task<ApiResponse<List<TournamentDto>>?> GetAvailableTournamentsAsync()
        {
            await EnsureAuthenticatedAsync();
            try
            {
                var response = await _httpClient.GetAsync("tournaments/available");
                return await ParseApiResponseAsync<List<TournamentDto>>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting available tournaments");
                return null;
            }
        }

        public async Task<ApiResponse<TournamentDto>?> CreateTournamentAsync(CreateTournamentRequest request, Guid createdBy)
        {
            await EnsureAuthenticatedAsync();
            try
            {
                var json = JsonSerializer.Serialize(request, GetJsonOptions());
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"tournaments?createdBy={createdBy}", content);

                return await ParseApiResponseAsync<TournamentDto>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating tournament");
                return null;
            }
        }

        public async Task<ApiResponse<GlobalMatchMetricsDto>?> GetGlobalMatchMetricsAsync()
        {
            await EnsureAuthenticatedAsync();
            try
            {
                var response = await _httpClient.GetAsync("metrics/global");
                return await ParseApiResponseAsync<GlobalMatchMetricsDto>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting global match metrics");
                return null;
            }
        }

        public async Task<ApiResponse<UserMatchMetricsDto>?> GetUserMatchMetricsAsync(Guid userId)
        {
            await EnsureAuthenticatedAsync();
            try
            {
                var response = await _httpClient.GetAsync($"metrics/user/{userId}");
                return await ParseApiResponseAsync<UserMatchMetricsDto>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting match metrics for user {UserId}", userId);
                return null;
            }
        }

        public async Task<ApiResponse<DuelForfeitResult>?> ForfeitDuelAsync(Guid duelId, Guid userId, ForfeitReason reason)
        {
            await EnsureAuthenticatedAsync();
            try
            {
                var request = new ForfeitDuelRequest { Reason = reason };
                var response = await _httpClient.PostAsync($"duels/{duelId}/forfeit?userId={userId}",
                    new StringContent(JsonSerializer.Serialize(request, GetJsonOptions()), Encoding.UTF8, "application/json"));
                return await ParseApiResponseAsync<DuelForfeitResult>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error forfeiting duel {DuelId} by user {UserId}", duelId, userId);
                return null;
            }
        }

        private static async Task<ApiResponse<T>?> ParseApiResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
                return null;

            return JsonSerializer.Deserialize<ApiResponse<T>>(content, GetJsonOptions());
        }

        private static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }
    }
}
