using AutoMapper;
using DatHost.Models;
using DatHostApi;
using DatHostApi.Models;
using GGHubDb;
using GGHubDb.Models;
using GGHubDb.Repos;
using GGHubShared.Enums;
using GGHubShared.Models;

namespace GGHubApi.Services
{
    public interface IDathostService
    {
        Task<DathostServerResult> CreateDuelServerAsync(
            Guid userId,
            Guid duelId,
            DuelFormat format,
            string selectedMap,
            List<string> playerSteamIds,
            bool primeOnly,
            int? customWarmupMinutes = null,
            int? customTickrate = null,
            int? customMaxRounds = null,
            bool? customOvertimeEnabled = null,
            string? preferredRegion = null);
        Task<DathostServerResult> StartDuelMatchAsync(Duel duel);
        Task<List<string>> GetDuelServerStatusAsync(string serverId);
        //Task<DathostServerResult> CreateTournamentServerAsync(Guid tournamentId, Guid matchId, string mapName, List<string> playerSteamIds);
        Task<DathostServerResult> StartMatchAsync(Guid matchId, string serverId);
        Task<bool> StopServerAsync(string serverId);
        Task<DathostMatchResult> GetMatchResultAsync(string matchId);
        Task<bool> HandleMatchCompletionAsync(Guid tournamentMatchId, DathostMatchResult result);

        Task<DathostServerResult> UpdateServerForDuelAsync(
            string serverId,
            Guid newDuelId,
            DuelFormat format,
            string selectedMap,
            List<string> playerSteamIds,
            bool primeOnly,
            int? customWarmupMinutes = null,
            int? customTickrate = null,
            int? customMaxRounds = null,
            bool? customOvertimeEnabled = null,
            CancellationToken cancellationToken = default);

        Task<string> GetServerConsoleAsync(string serverId, int maxLines = 500, CancellationToken cancellationToken = default);
        Task SendConsoleCommandAsync(string serverId, string command, CancellationToken cancellationToken = default);
    }

    public class DathostService : IDathostService
    {
        private readonly IGameServerService _gameServerService;
        private readonly ICs2MatchService _cs2MatchService;
        private readonly IMatchConfigService _matchConfigService;
        private readonly IServerService _regionService;
        private readonly IUserRepository _userRepository;
        private readonly IDuelRepository _duelRepository;
        private readonly ILogger<DathostService> _logger;
        private readonly IMapper _mapper;
        private readonly string _webhookUrl;
        private readonly string _webhookSecret;

        public DathostService(
            IGameServerService gameServerService,
            ICs2MatchService cs2MatchService,
            IMatchConfigService matchConfigService,
            IServerService regionService,
            IUserRepository userRepository,
            IDuelRepository duelRepository,
            AppDbContext context,
            ILogger<DathostService> logger,
            IConfiguration configuration,
            IMapper mapper)
        {
            _gameServerService = gameServerService;
            _cs2MatchService = cs2MatchService;
            _matchConfigService = matchConfigService;
            _regionService = regionService;
            _userRepository = userRepository;
            _duelRepository = duelRepository;
            _logger = logger;
            _mapper = mapper;
            _webhookUrl = configuration["DatHost:WebhookUrl"] ?? "https://yourdomain.com/api/webhook/dathost";
            _webhookSecret = configuration["DatHost:WebhookSecret"] ?? throw new Exception("WebhookSecret is null");
        }

        public async Task<DathostServerResult> CreateDuelServerAsync(
            Guid userId,
            Guid duelId,
            DuelFormat format,
            string selectedMap,
            List<string> playerSteamIds,
            bool primeOnly,
            int? customWarmupMinutes = null,
            int? customTickrate = null,
            int? customMaxRounds = null,
            bool? customOvertimeEnabled = null,
            string? preferredRegion = null)
        {
            try
            {
                _logger.LogInformation("Creating server for duel {DuelId}, format {Format}", duelId, format);

                var user = await _userRepository.GetByIdAsync(userId);
                var location = await _regionService.GetOptimalLocationAsync(user?.Country, preferredRegion);
                var formatConfig = await _matchConfigService.GetDuelConfigAsync(format);
                //var serverConfig = await _matchConfigService.GenerateServerConfigAsync(format, primeOnly, customWarmupMinutes, customTickrate, customMaxRounds, customOvertimeEnabled);

                var finalTickrate = customTickrate ?? formatConfig.DefaultTickrate;

                var serverRequest = new CreateGameServerRequest
                {
                    Name = $"Duel-{duelId}",
                    Game = "cs2",
                    Location = location,
                    Autostop = true,
                    AutostopMinutes = formatConfig.AutostopMinutes,
                    Cs2Settings = new Cs2Settings
                    {
                        Slots = Math.Max(playerSteamIds.Count + 2, 5), // +2 для запасу, мінімум 5,
                        MapgroupStartMap = selectedMap,
                        GameMode = "competitive",
                        Tickrate = finalTickrate,

                        Rcon = GenerateRandomPassword(12),
                        MapsSource = "mapgroup",
                        //Config = serverConfig,
                        // Legacy values for compatibility
                        MapName = selectedMap,
                        Players = playerSteamIds.Count
                    }
                };

                var gameServer = await _gameServerService.CreateGameServerAsync(serverRequest);

                return MapServerToResult(gameServer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating server for duel {DuelId}", duelId);
                return new DathostServerResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<DathostServerResult> StartDuelMatchAsync(Duel duel)
        {
            var serverId = duel.GameServer?.ExternalServerId;
            try
            {
                if (string.IsNullOrEmpty(serverId))
                {
                    return new DathostServerResult
                    {
                        Success = false,
                        ErrorMessage = "Duel does not have an associated server"
                    };
                }
                var server = await _gameServerService.GetGameServerAsync(serverId);
                if (!server.Status.Contains("online"))
                {
                    await _gameServerService.StartGameServerAsync(serverId);
                    await Task.Delay(1000);
                }

                if (!string.IsNullOrEmpty(server.MatchId))
                {
                    return MapServerToResult(server);
                }

                var map = duel.Maps.OrderBy(m => m.Order).FirstOrDefault()?.MapName
                           ?? server.Cs2Settings?.MapgroupStartMap
                           ?? "de_dust2";

                var players = CreatePlayersList(duel);

                var cs2MatchRequest = new CreateCs2MatchRequest
                {
                    GameServerId = serverId,
                    Players = players,

                    // ✅ Опціональні команди
                    //Team1 = new TeamInfo { Name = "Team 1", Flag = "US" },
                    //Team2 = new TeamInfo { Name = "Team 2", Flag = "EU" },

                    //// ✅ Опціональні налаштування
                    //Settings = new MatchSettings
                    //{
                    //    Map = map,
                    //    ConnectTime = 300,
                    //    MatchBeginCountdown = 30,
                    //    WaitForGotv = false,
                    //    EnablePlugin = true,
                    //    EnableTechPause = true
                    //},

                    Webhooks = new WebhookSettings
                    {
                        //PlayerVotekickSuccessUrl = _webhookUrl,
                        EventUrl = _webhookUrl,
                        EnabledEvents = new List<string>
                        {
                            "server_ready_for_players",
                            "match_started",
                            "player_connected",      
                            "player_disconnected",
                            "match_ended",
                            "round_end"
                        },
                        AuthorizationHeader = "Bearer " + _webhookSecret
                    }
                };

                var cs2Match = await _cs2MatchService.StartCs2MatchAsync(cs2MatchRequest);


                
                await UpdateDuelWithMatchId(duel.Id, cs2Match.Id); // Зберегти match ID для webhook'ів

                return MapServerToResult(server, cs2Match.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting duel server {ServerId}", serverId);
                return new DathostServerResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<List<string>> GetDuelServerStatusAsync(string serverId)
        {
            var server = await _gameServerService.GetGameServerAsync(serverId);
            return server.Status ?? new List<string>();
        }

        //public async Task<DathostServerResult> CreateTournamentServerAsync(Guid tournamentId, Guid matchId, string mapName, List<string> playerSteamIds)
        //{
        //    try
        //    {
        //        _logger.LogInformation("Creating server for tournament {TournamentId}, match {MatchId}", tournamentId, matchId);

        //        var location = await _regionService.GetOptimalLocationAsync(null);
        //        var cfg = await _matchConfigService.GenerateServerConfigAsync(DuelFormat.FiveVsFive, true);

        //        var serverRequest = new CreateGameServerRequest
        //        {
        //            Name = $"Tournament-{tournamentId}-Match-{matchId}",
        //            Game = "cs2",
        //            Autostop = true,
        //            AutostopMinutes = 30,
        //            Location = location,
        //            Cs2Settings = new Cs2Settings
        //            {
        //                Slots = playerSteamIds.Count,
        //                MapgroupStartMap = mapName,
        //                GameMode = "competitive",
        //                Tickrate = 128,

        //                Rcon = GenerateRandomPassword(12),
        //                MapsSource = "mapgroup",
        //                Config = cfg,
        //                MapName = mapName,
        //                Players = playerSteamIds.Count
        //            }
        //        };

        //        var gameServer = await _gameServerService.CreateGameServerAsync(serverRequest);

        //        await _gameServerService.StartGameServerAsync(gameServer.Id);

        //        //var cs2MatchRequest = new CreateCs2MatchRequest
        //        //{
        //        //    GameServerId = gameServer.Id,
        //        //    Maps = new List<string> { mapName },
        //        //    GameSettings = new GameSettings
        //        //    {
        //        //        Warmup = true
        //        //    },
        //        //    Webhooks = new MatchWebhooks
        //        //    {
        //        //        Url = _webhookUrl,
        //        //        Secret = GenerateRandomPassword(16)
        //        //    }
        //        //};

        //        //var cs2Match = await _cs2MatchService.StartCs2MatchAsync(cs2MatchRequest);

        //        //await AddPlayersToMatch(cs2Match.Id, playerSteamIds);

        //        _logger.LogInformation("Server created successfully for match {MatchId}: {ServerId}", matchId, gameServer.Id);

        //        return MapServerToResult(gameServer, cs2Match.Id);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating server for tournament {TournamentId}, match {MatchId}", tournamentId, matchId);
        //        return new DathostServerResult
        //        {
        //            Success = false,
        //            ErrorMessage = ex.Message
        //        };
        //    }
        //}

        public async Task<DathostServerResult> StartMatchAsync(Guid matchId, string serverId)
        {
            try
            {
                _logger.LogInformation("Starting match {MatchId} on server {ServerId}", matchId, serverId);

                var server = await _gameServerService.GetGameServerAsync(serverId);
                if (!server.Status.Contains("online"))
                {
                    await _gameServerService.StartGameServerAsync(serverId);
                    await Task.Delay(10000); // Wait for server to start
                }

                if (!string.IsNullOrEmpty(server.MatchId))
                {
                    await _cs2MatchService.StartKnifeRoundAsync(server.MatchId);
                }

                return MapServerToResult(server);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting match {MatchId} on server {ServerId}", matchId, serverId);
                return new DathostServerResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<bool> StopServerAsync(string serverId)
        {
            try
            {
                _logger.LogInformation("Stopping server {ServerId}", serverId);

                await _gameServerService.StopGameServerAsync(serverId);
                await _gameServerService.DeleteGameServerAsync(serverId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping server {ServerId}", serverId);
                return false;
            }
        }

        public async Task<DathostMatchResult> GetMatchResultAsync(string matchId)
        {
            try
            {
                var match = await _cs2MatchService.GetCs2MatchAsync(matchId);

                return new DathostMatchResult
                {
                    Success = true,
                    MatchId = matchId,
                    Status = match.Status,
                    WinnerTeam = match.Result?.Winner,
                    Team1Score = match.Result?.Team1Score ?? 0,
                    Team2Score = match.Result?.Team2Score ?? 0,
                    CompletedAt = match.Ended
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting match result for {MatchId}", matchId);
                return new DathostMatchResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<bool> HandleMatchCompletionAsync(Guid tournamentMatchId, DathostMatchResult result)
        {
            try
            {
                _logger.LogInformation("Handling match completion for tournament match {MatchId}", tournamentMatchId);

                // This will be handled by the tournament service
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling match completion for {MatchId}", tournamentMatchId);
                return false;
            }
        }

        private string GenerateRandomPassword(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private DathostServerResult MapServerToResult(GameServerResponse server, string? matchId = null)
        {
            var result = _mapper.Map<DathostServerResult>(server);
            if (matchId is not null)
            {
                result.MatchId = matchId;
            }

            return result;
        }

    

        public async Task<DathostServerResult> UpdateServerForDuelAsync(
            string serverId,
            Guid newDuelId,
            DuelFormat format,
            string selectedMap,
            List<string> playerSteamIds,
            bool primeOnly,
            int? customWarmupMinutes = null,
            int? customTickrate = null,
            int? customMaxRounds = null,
            bool? customOvertimeEnabled = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating existing server {ServerId} for duel {DuelId}", serverId, newDuelId);

                var formatConfig = await _matchConfigService.GetDuelConfigAsync(format, cancellationToken);
                var serverConfig = await _matchConfigService.GenerateServerConfigAsync(
                    format, primeOnly, customWarmupMinutes, customTickrate, customMaxRounds, customOvertimeEnabled);

                var finalTickrate = customTickrate ?? formatConfig.DefaultTickrate;


                var updateRequest = new CreateGameServerRequest
                {
                    Cs2Settings = new Cs2Settings
                    {
                        Slots = Math.Max(playerSteamIds.Count + 2, 5),
                        MapgroupStartMap = selectedMap,
                        GameMode = "competitive",
                        Tickrate = finalTickrate,
                        Rcon = GenerateRandomPassword(12),
                        MapsSource = "mapgroup",
                        Config = serverConfig,
                        MapName = selectedMap,
                        Players = playerSteamIds.Count
                    }
                };

                await _gameServerService.UpdateGameServerAsync(serverId, updateRequest);

                // Перезапустити сервер з новими налаштуваннями
                await _gameServerService.StopGameServerAsync(serverId);
                //await Task.Delay(5000, cancellationToken); // Дати час на зупинку
                //await _gameServerService.StartGameServerAsync(serverId);

                var serverInfo = await _gameServerService.GetGameServerAsync(serverId);

                return MapServerToResult(serverInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating server {ServerId} for duel {DuelId}", serverId, newDuelId);
                return new DathostServerResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        private List<CreateMatchPlayer> CreatePlayersList(Duel duel)
        {
            var participants = duel.Participants
                .Where(p => !string.IsNullOrEmpty(p.User.SteamId))
                .ToList();

            var players = new List<CreateMatchPlayer>();

            foreach (var participant in participants)
            {
                players.Add(new CreateMatchPlayer
                {
                    SteamId64 = participant.User.SteamId!,
                    Team = participant.Team == 1 ? "team1" : "team2", 
                    NicknameOverride = GetPlayerDisplayName(participant.User)
                });
            }

            return players;
        }
        private string GetPlayerDisplayName(User user)
        {
            return !string.IsNullOrEmpty(user.Username) ? user.Username : user.SteamId ?? "Unknown";
        }

        private async Task UpdateDuelWithMatchId(Guid duelId, string matchId)
        {
            await _duelRepository.UpdateGameServerMatchIdAsync(duelId, matchId);
        }

        public async Task<string> GetServerConsoleAsync(string serverId, int maxLines = 500, CancellationToken cancellationToken = default)
        {
            return await _gameServerService.GetServerConsoleAsync(serverId, maxLines, cancellationToken);
        }

        public async Task SendConsoleCommandAsync(string serverId, string command, CancellationToken cancellationToken = default)
        {
            await _gameServerService.SendConsoleCommandAsync(serverId, command, cancellationToken);
        }
    }
}