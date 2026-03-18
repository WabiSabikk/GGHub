using GGHubShared.Models;
using GGHubShared.Enums;
using System.Text.Json;

namespace GGHubApi.Services
{
    public interface ISteamUserService
    {
        Task<ApiResponse<SteamUserInfo>> GetSteamUserInfoAsync(string steamId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ValidateSteamIdAsync(string steamId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<SteamUserInfo>>> GetMultipleSteamUsersAsync(List<string> steamIds, CancellationToken cancellationToken = default);
        Task<ApiResponse<SteamUserGames>> GetUserGamesAsync(string steamId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> HasCS2GameAsync(string steamId, CancellationToken cancellationToken = default);
    }

    public class SteamUserService : ISteamUserService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<SteamUserService> _logger;
        private const string STEAM_API_BASE = "https://api.steampowered.com";
        private const int CS2_APP_ID = 730; // Counter-Strike 2 App ID

        public SteamUserService(HttpClient httpClient, IConfiguration configuration, ILogger<SteamUserService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Steam:ApiKey"] ?? throw new InvalidOperationException("Steam API key not configured");
            _logger = logger;
        }

        public async Task<ApiResponse<SteamUserInfo>> GetSteamUserInfoAsync(string steamId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!IsValidSteamId(steamId))
                {
                    return new ApiResponse<SteamUserInfo>
                    {
                        Success = false,
                        Message = "Invalid Steam ID format"
                    };
                }

                var url = $"{STEAM_API_BASE}/ISteamUser/GetPlayerSummaries/v2/?key={_apiKey}&steamids={steamId}";
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Steam API request failed with status: {StatusCode}", response.StatusCode);
                    return new ApiResponse<SteamUserInfo>
                    {
                        Success = false,
                        Message = "Failed to fetch user info from Steam API"
                    };
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var steamResponse = JsonSerializer.Deserialize<SteamUserResponse>(content);

                if (steamResponse?.Response?.Players?.Any() != true)
                {
                    return new ApiResponse<SteamUserInfo>
                    {
                        Success = false,
                        Message = "Steam user not found"
                    };
                }

                var player = steamResponse.Response.Players.First();
                var userInfo = MapSteamPlayerToUserInfo(player);

                return new ApiResponse<SteamUserInfo>
                {
                    Success = true,
                    Data = userInfo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Steam user info for Steam ID: {SteamId}", steamId);
                return new ApiResponse<SteamUserInfo>
                {
                    Success = false,
                    Code = ErrorCode.ServerError
                };
            }
        }

        public async Task<ApiResponse<bool>> ValidateSteamIdAsync(string steamId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!IsValidSteamId(steamId))
                {
                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Data = false,
                        Message = "Invalid Steam ID format"
                    };
                }

                var userResult = await GetSteamUserInfoAsync(steamId, cancellationToken);
                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = userResult.Success,
                    Message = userResult.Success ? "Valid Steam ID" : "Steam ID not found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Steam ID: {SteamId}", steamId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Code = ErrorCode.ServerError
                };
            }
        }

        public async Task<ApiResponse<List<SteamUserInfo>>> GetMultipleSteamUsersAsync(List<string> steamIds, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!steamIds.Any() || steamIds.Count > 100) // Steam API limit
                {
                    return new ApiResponse<List<SteamUserInfo>>
                    {
                        Success = false,
                        Message = "Invalid number of Steam IDs (must be 1-100)"
                    };
                }

                var validSteamIds = steamIds.Where(IsValidSteamId).ToList();
                if (!validSteamIds.Any())
                {
                    return new ApiResponse<List<SteamUserInfo>>
                    {
                        Success = false,
                        Message = "No valid Steam IDs provided"
                    };
                }

                var steamIdsString = string.Join(",", validSteamIds);
                var url = $"{STEAM_API_BASE}/ISteamUser/GetPlayerSummaries/v2/?key={_apiKey}&steamids={steamIdsString}";

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<List<SteamUserInfo>>
                    {
                        Success = false,
                        Message = "Failed to fetch users info from Steam API"
                    };
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var steamResponse = JsonSerializer.Deserialize<SteamUserResponse>(content);

                var users = steamResponse?.Response?.Players?
                    .Select(MapSteamPlayerToUserInfo)
                    .ToList() ?? new List<SteamUserInfo>();

                return new ApiResponse<List<SteamUserInfo>>
                {
                    Success = true,
                    Data = users
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting multiple Steam users info");
                return new ApiResponse<List<SteamUserInfo>>
                {
                    Success = false,
                    Code = ErrorCode.ServerError
                };
            }
        }

        public async Task<ApiResponse<SteamUserGames>> GetUserGamesAsync(string steamId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!IsValidSteamId(steamId))
                {
                    return new ApiResponse<SteamUserGames>
                    {
                        Success = false,
                        Message = "Invalid Steam ID format"
                    };
                }

                var url = $"{STEAM_API_BASE}/IPlayerService/GetOwnedGames/v1/?key={_apiKey}&steamid={steamId}&include_appinfo=1&include_played_free_games=1";
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<SteamUserGames>
                    {
                        Success = false,
                        Message = "Failed to fetch games from Steam API"
                    };
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var gamesResponse = JsonSerializer.Deserialize<SteamGamesResponse>(content);

                var userGames = new SteamUserGames
                {
                    SteamId = steamId,
                    GameCount = gamesResponse?.Response?.GameCount ?? 0,
                    Games = gamesResponse?.Response?.Games?.Select(g => new SteamGame
                    {
                        AppId = g.AppId,
                        Name = g.Name ?? "",
                        PlaytimeForever = g.PlaytimeForever,
                        PlaytimeTwoWeeks = g.PlaytimeTwoWeeks
                    }).ToList() ?? new List<SteamGame>()
                };

                return new ApiResponse<SteamUserGames>
                {
                    Success = true,
                    Data = userGames
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Steam user games for Steam ID: {SteamId}", steamId);
                return new ApiResponse<SteamUserGames>
                {
                    Success = false,
                    Code = ErrorCode.ServerError
                };
            }
        }

        public async Task<ApiResponse<bool>> HasCS2GameAsync(string steamId, CancellationToken cancellationToken = default)
        {
            try
            {
                var gamesResult = await GetUserGamesAsync(steamId, cancellationToken);

                if (!gamesResult.Success || gamesResult.Data == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Could not check user's games"
                    };
                }

                var hasCS2 = gamesResult.Data.Games.Any(g => g.AppId == CS2_APP_ID);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = hasCS2,
                    Message = hasCS2 ? "User owns CS2" : "User does not own CS2"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking CS2 ownership for Steam ID: {SteamId}", steamId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Code = ErrorCode.ServerError
                };
            }
        }

        private static bool IsValidSteamId(string steamId)
        {
            // Steam ID should be a 17-digit number
            return !string.IsNullOrEmpty(steamId) &&
                   steamId.All(char.IsDigit) &&
                   steamId.Length == 17 &&
                   steamId.StartsWith("7656119");
        }

        private static SteamUserInfo MapSteamPlayerToUserInfo(SteamPlayer player)
        {
            return new SteamUserInfo
            {
                SteamId = player.SteamId ?? "",
                PersonaName = player.PersonaName ?? "",
                ProfileUrl = player.ProfileUrl ?? "",
                Avatar = player.Avatar ?? "",
                AvatarMedium = player.AvatarMedium ?? "",
                AvatarFull = player.AvatarFull ?? "",
                PersonaState = player.PersonaState,
                CommunityVisibilityState = player.CommunityVisibilityState,
                IsOnline = (player.PersonaState) > 0,
                IsPublicProfile = (player.CommunityVisibilityState) == 3
            };
        }
    }

    // Extended DTOs for Steam data


    public class SteamUserGames
    {
        public string SteamId { get; set; } = string.Empty;
        public int GameCount { get; set; }
        public List<SteamGame> Games { get; set; } = new();
    }

    public class SteamGame
    {
        public int AppId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PlaytimeForever { get; set; }
        public int? PlaytimeTwoWeeks { get; set; }
    }

    // Steam API Response Models
    public class SteamGamesResponse
    {
        public SteamGamesData? Response { get; set; }
    }

    public class SteamGamesData
    {
        public int GameCount { get; set; }
        public List<SteamGameData>? Games { get; set; }
    }

    public class SteamGameData
    {
        public int AppId { get; set; }
        public string? Name { get; set; }
        public int PlaytimeForever { get; set; }
        public int? PlaytimeTwoWeeks { get; set; }
    }
}