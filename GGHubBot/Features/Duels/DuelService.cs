using GGHubBot.Models;
using GGHubBot.Services;
using GGHubShared.Enums;
using GGHubShared.Models;

namespace GGHubBot.Features.Duels
{
    public class DuelService
    {
        private readonly ApiService _apiService;
        private readonly UserStateService _userStateService;

        public DuelService(ApiService apiService, UserStateService userStateService)
        {
            _apiService = apiService;
            _userStateService = userStateService;
        }

        public async Task<CreateDuelState?> GetDuelStateAsync(long telegramId)
        {
            return await _userStateService.GetStateDataAsync<CreateDuelState>(telegramId);
        }

        public async Task UpdateDuelStateAsync(long telegramId, BotState state, CreateDuelState duelState)
        {
            await _userStateService.UpdateUserStateAsync(telegramId, state, duelState);
        }

        public async Task<ApiResponse<DuelDto>?> CreateDuelAsync(CreateDuelState duelState, Guid userId)
        {
            var request = new CreateDuelRequest
            {
                Title = $"{duelState.Format} Duel",
                Format = ParseDuelFormat(duelState.Format!),
                RoundFormat = ParseRoundFormat(duelState.RoundFormat!),
                EntryFee = duelState.EntryFee!.Value,
                Maps = duelState.Maps,
                PrimeOnly = duelState.PrimeOnly,
                WarmupMinutes = duelState.WarmupMinutes,
                CustomMaxRounds = duelState.MaxRounds
            };

            return await _apiService.CreateDuelAsync(request, userId);
        }

        public async Task<ApiResponse<string>?> GenerateInviteLinkAsync(Guid duelId)
        {
            return await _apiService.GenerateDuelInviteLinkAsync(duelId);
        }

        public async Task<ApiResponse<List<DuelDto>>?> GetAvailableDuelsAsync()
        {
            return await _apiService.GetAvailableDuelsAsync();
        }

        public async Task<ApiResponse<List<DuelDto>>?> GetUserDuelsAsync(Guid userId)
        {
            return await _apiService.GetUserDuelsAsync(userId);
        }

        public async Task<ApiResponse<DuelDto>?> JoinDuelAsync(Guid duelId, Guid userId)
        {
            return await _apiService.JoinDuelAsync(duelId, userId);
        }

        public async Task<ApiResponse<DuelDto>?> JoinDuelByCodeAsync(string code, Guid userId)
        {
            return await _apiService.JoinDuelByCodeAsync(code, userId);
        }

        public async Task<ApiResponse<DuelDto>?> GetFullDuelAsync(Guid duelId)
        {
            return await _apiService.GetFullDuelAsync(duelId);
        }

        public async Task<ApiResponse<OpponentStatus>> ConfirmReadyAsync(Guid duelId, Guid userId)
        {
            return await _apiService.ConfirmReadyAsync(duelId, userId);
        }

        public bool IsValidStep(CreateDuelState? state, int expected)
        {
            return state != null && state.CurrentStep == expected;
        }

        private static DuelFormat ParseDuelFormat(string format) => format switch
        {
            "1v1" => DuelFormat.OneVsOne,
            "2v2" => DuelFormat.TwoVsTwo,
            "5v5" => DuelFormat.FiveVsFive,
            _ => DuelFormat.OneVsOne
        };

        private static RoundFormat ParseRoundFormat(string roundFormat) => roundFormat switch
        {
            "bo1" => RoundFormat.BestOfOne,
            "bo3" => RoundFormat.BestOfThree,
            "bo5" => RoundFormat.BestOfFive,
            _ => RoundFormat.BestOfOne
        };

        public static int GetPlayersCount(string format) => format switch
        {
            "1v1" => 2,
            "2v2" => 4,
            "5v5" => 10,
            _ => 2
        };

        public async Task<ApiResponse<DuelForfeitResult>?> ForfeitDuelAsync(Guid duelId, Guid userId, ForfeitReason reason)
        {
            return await _apiService.ForfeitDuelAsync(duelId, userId, reason);
        }
    }
}
