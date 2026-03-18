using GGHubApi.Hubs;
using GGHubDb.Models;
using GGHubDb.Repos;
using GGHubShared.Enums;
using GGHubShared.Models;

namespace GGHubApi.Services
{
    public interface IDathostWebhookService
    {
        Task<ApiResponse<bool>> ProcessMatchEventAsync(DathostMatchWebhook payload, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ProcessMatchWebhookAsync(DathostMatchWebhook payload, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ProcessVotekickEventAsync(DathostVotekickWebhook payload, CancellationToken cancellationToken = default);
    }

    public class DathostWebhookService : IDathostWebhookService
    {
        private readonly IDuelRepository _duelRepository;
        private readonly ITournamentMatchRepository _matchRepository;
        private readonly IDuelService _duelService;
        private readonly ITournamentService _tournamentService;
        private readonly IDuelHubService _duelHubService;
        private readonly ITournamentHubService _tournamentHubService;
        private readonly IUserRepository _userRepository;
        private readonly IDathostService _dathostService;
        private readonly ILogger<DathostWebhookService> _logger;

        public DathostWebhookService(
            IDuelRepository duelRepository,
            ITournamentMatchRepository matchRepository,
            IDuelService duelService,
            ITournamentService tournamentService,
            IDuelHubService duelHubService,
            ITournamentHubService tournamentHubService,
            IUserRepository userRepository,
            IDathostService dathostService,
            ILogger<DathostWebhookService> logger)
        {
            _duelRepository = duelRepository;
            _matchRepository = matchRepository;
            _duelService = duelService;
            _tournamentService = tournamentService;
            _duelHubService = duelHubService;
            _tournamentHubService = tournamentHubService;
            _userRepository = userRepository;
            _dathostService = dathostService;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> ProcessMatchEventAsync(DathostMatchWebhook payload, CancellationToken cancellationToken = default)
        {
            var duel = await _duelRepository.GetByExternalMatchIdAsync(payload.Id, cancellationToken);
            if (duel != null)
            {
                var result = new DathostMatchResult
                {
                    Success = true,
                    MatchId = payload.Id,
                    Status = payload.Status,
                    WinnerTeam = DetermineWinnerTeam(payload),
                    Team1Score = payload.Team1?.Stats?.Score ?? 0,
                    Team2Score = payload.Team2?.Stats?.Score ?? 0,
                    CompletedAt = DateTime.UtcNow
                };

                return await _duelService.ProcessMatchResultAsync(duel.Id, result, cancellationToken);
            }

            var match = await _matchRepository.GetByExternalMatchIdAsync(payload.Id, cancellationToken);
            if (match != null)
            {
                var result = new DathostMatchResult
                {
                    Success = true,
                    MatchId = payload.Id,
                    Status = payload.Status,
                    WinnerTeam = DetermineWinnerTeam(payload),
                    Team1Score = payload.Team1?.Stats?.Score ?? 0,
                    Team2Score = payload.Team2?.Stats?.Score ?? 0,
                    CompletedAt = DateTime.UtcNow
                };

                return await _tournamentService.ProcessMatchResultAsync(match.Id, result, cancellationToken);
            }

            _logger.LogWarning("Match not found for DatHost webhook: {MatchId}", payload.Id);
            return new ApiResponse<bool> { Success = false, Data = false, Message = "Match not found" };
        }

        public async Task<ApiResponse<bool>> ProcessMatchWebhookAsync(DathostMatchWebhook payload, CancellationToken cancellationToken = default)
        {
            // Перевіряємо чи це event-driven webhook (містить масив подій)
            if (payload.Events != null && payload.Events.Any())
            {
                // Знаходимо останню подію для обробки
                var lastEvent = payload.Events.OrderByDescending(e => e.Timestamp).First();
                _logger.LogInformation("Processing event-driven webhook: {Event} for match {MatchId}", lastEvent.Event, payload.Id);

                // Обробляємо подію з повними даними матчу
                await ProcessEventAsync(lastEvent.Event, payload, lastEvent.Payload, cancellationToken);

                return new ApiResponse<bool> { Success = true, Data = true };
            }

            // Legacy webhook - обробляємо як завершення матчу
            var duel = await _duelRepository.GetByExternalMatchIdAsync(payload.Id, cancellationToken);
            if (duel != null)
            {
                return await ProcessDuelMatchResultAsync(duel, payload, cancellationToken);
            }

            var match = await _matchRepository.GetByExternalMatchIdAsync(payload.Id, cancellationToken);
            if (match != null)
            {
                return await ProcessTournamentMatchResultAsync(match, payload, cancellationToken);
            }

            _logger.LogWarning("Match not found for DatHost webhook: {MatchId}", payload.Id);
            return new ApiResponse<bool> { Success = false, Data = false, Message = "Match not found" };
        }

        public async Task<ApiResponse<bool>> ProcessVotekickEventAsync(DathostVotekickWebhook payload, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Player votekicked - Match: {MatchId}, Steam: {SteamId}, Team: {Team}",
                payload.MatchId, payload.SteamId64, payload.Team);

            return new ApiResponse<bool> { Success = true, Data = true };
        }

        private async Task ProcessEventAsync(string eventType, DathostMatchWebhook matchData, DathostEventPayload? eventPayload, CancellationToken cancellationToken)
        {
            switch (eventType)
            {
                case "booting_server":
                    await UpdateGameServerStatusAsync(matchData.Id, ServerStatus.Creating, cancellationToken);
                    break;
                case "loading_map":
                    await UpdateGameServerStatusAsync(matchData.Id, ServerStatus.Starting, cancellationToken);
                    break;
                case "server_ready_for_players":
                    await HandleServerReadyAsync(matchData, cancellationToken);
                    break;
                case "all_players_connected":
                    _logger.LogInformation("All players connected for match: {MatchId}", matchData.Id);
                    break;
                case "match_started":
                    await HandleMatchStartedAsync(matchData, cancellationToken);
                    break;
                case "round_end":
                    await HandleRoundEndAsync(matchData, eventPayload, cancellationToken);
                    break;
                case "match_ended":
                    await HandleMatchEndedAsync(matchData, cancellationToken);
                    break;
                case "players_exited":
                    await UpdateGameServerStatusAsync(matchData.Id, ServerStatus.Stopping, cancellationToken);
                    break;
                case "gotv_stopped":
                    _logger.LogInformation("GOTV stopped for match: {MatchId}", matchData.Id);
                    break;
                case "player_connected":
                    if (eventPayload?.SteamId64 != null)
                    {
                        _logger.LogInformation("Player connected: {SteamId} to match: {MatchId}", eventPayload.SteamId64, matchData.Id);
                    }
                    break;
                case "player_disconnected":
                    if (eventPayload?.SteamId64 != null)
                    {
                        _logger.LogWarning("Player disconnected: {SteamId} from match: {MatchId}", eventPayload.SteamId64, matchData.Id);
                    }
                    break;
                case "match_canceled":
                    await HandleMatchCanceledAsync(matchData, cancellationToken);
                    break;
                default:
                    _logger.LogInformation("Unhandled DatHost event: {Event}", eventType);
                    break;
            }
        }

        private async Task HandleServerReadyAsync(DathostMatchWebhook matchData, CancellationToken cancellationToken)
        {
            var duel = await _duelRepository.GetByExternalMatchIdAsync(matchData.Id, cancellationToken);
            if (duel?.GameServer != null)
            {
                await _duelHubService.NotifyServerCreated(duel.Id, new GameServerDto
                {
                    ServerIp = duel.GameServer.ServerIp!,
                    ServerPort = duel.GameServer.ServerPort!.Value,
                    Password = matchData.Settings?.Password ?? duel.GameServer.Password
                });
                return;
            }

            var match = await _matchRepository.GetByExternalMatchIdAsync(matchData.Id, cancellationToken);
            if (match != null && !string.IsNullOrEmpty(match.ServerIp))
            {
                await _tournamentHubService.NotifyServerReady(match.Id, match.ServerIp, match.ServerPort!.Value,
                    matchData.Settings?.Password ?? match.ServerPassword!);
            }
        }

        private async Task HandleMatchStartedAsync(DathostMatchWebhook matchData, CancellationToken cancellationToken)
        {
            var duel = await _duelRepository.GetByExternalMatchIdAsync(matchData.Id, cancellationToken);
            if (duel != null)
            {
                await _duelRepository.UpdateStatusAsync(duel.Id, DuelStatus.InProgress, cancellationToken);
                return;
            }

            var match = await _matchRepository.GetByExternalMatchIdAsync(matchData.Id, cancellationToken);
            if (match != null)
            {
                await _matchRepository.UpdateMatchStatusAsync(match.Id, TournamentMatchStatus.InProgress, cancellationToken);
                await _tournamentHubService.NotifyMatchStatusChanged(match.Id, TournamentMatchStatus.InProgress);
            }
        }

        private async Task HandleRoundEndAsync(DathostMatchWebhook matchData, DathostEventPayload? eventPayload, CancellationToken cancellationToken)
        {
            if (eventPayload == null) return;

            var team1Score = eventPayload.Team1Score ?? 0;
            var team2Score = eventPayload.Team2Score ?? 0;

            var duel = await _duelRepository.GetByExternalMatchIdAsync(matchData.Id, cancellationToken);
            if (duel != null)
            {
                await _duelHubService.NotifyRoundEndAsync(duel.Id, team1Score, team2Score);
                return;
            }

            var match = await _matchRepository.GetByExternalMatchIdAsync(matchData.Id, cancellationToken);
            if (match != null)
            {
                await _tournamentHubService.NotifyRoundEndAsync(match.Id, team1Score, team2Score);
            }
        }

        private async Task HandleMatchEndedAsync(DathostMatchWebhook matchData, CancellationToken cancellationToken)
        {
            // Тепер у нас є повні дані матчу з результатами
            var duel = await _duelRepository.GetByExternalMatchIdAsync(matchData.Id, cancellationToken);
                        if (duel != null)
            {
                await _duelRepository.UpdateStatusAsync(duel.Id, DuelStatus.Completed, cancellationToken);

                // Обробляємо результат матчу з повними даними
                await ProcessDuelMatchResultAsync(duel, matchData, cancellationToken);
                return;
            }

            var match = await _matchRepository.GetByExternalMatchIdAsync(matchData.Id, cancellationToken);
            if (match != null)
            {
                await _matchRepository.UpdateMatchStatusAsync(match.Id, TournamentMatchStatus.Completed, cancellationToken);
                await _tournamentHubService.NotifyMatchStatusChanged(match.Id, TournamentMatchStatus.Completed);

                // Обробляємо результат матчу з повними даними
                await ProcessTournamentMatchResultAsync(match, matchData, cancellationToken);
            }
        }

        private async Task HandleMatchCanceledAsync(DathostMatchWebhook matchData, CancellationToken cancellationToken)
        {
            var duel = await _duelRepository.GetByExternalMatchIdAsync(matchData.Id, cancellationToken);
            if (duel != null)
            {
                await _duelRepository.UpdateStatusAsync(duel.Id, DuelStatus.Cancelled, cancellationToken);
                return;
            }

            var match = await _matchRepository.GetByExternalMatchIdAsync(matchData.Id, cancellationToken);
            if (match != null)
            {
                await _matchRepository.UpdateMatchStatusAsync(match.Id, TournamentMatchStatus.Cancelled, cancellationToken);
            }
        }

        private async Task UpdateGameServerStatusAsync(string externalMatchId, ServerStatus status, CancellationToken cancellationToken)
        {
            var duel = await _duelRepository.GetByExternalMatchIdAsync(externalMatchId, cancellationToken);
            if (duel?.GameServer != null)
            {
                await _duelRepository.UpdateGameServerStatusAsync(duel.Id, status, cancellationToken);
            }
        }

        private async Task<ApiResponse<bool>> ProcessDuelMatchResultAsync(Duel duel, DathostMatchWebhook payload, CancellationToken cancellationToken)
        {
            var winnerTeam = DetermineWinnerTeam(payload);
            var team1Score = payload.Team1?.Stats?.Score ?? 0;
            var team2Score = payload.Team2?.Stats?.Score ?? 0;

            await UpdateDuelParticipantStats(duel, payload.Players, cancellationToken);

            if (payload.Finished && winnerTeam != "draw")
            {
                var winnerTeamNumber = winnerTeam == "team1" ? 1 : 2;
                var winner = duel.Participants.FirstOrDefault(p => p.Team == winnerTeamNumber);

                if (winner != null)
                {
                    await _duelRepository.CompleteDuelAsync(duel.Id, winner.UserId, cancellationToken);

                    foreach (var participant in duel.Participants)
                    {
                        var isWinner = participant.UserId == winner.UserId;
                        await _userRepository.UpdateStatsAsync(participant.UserId, isWinner, cancellationToken);

                        if (isWinner)
                        {
                            await _userRepository.UpdateBalanceAsync(participant.UserId, duel.PrizeFund, cancellationToken);
                        }
                    }

                    if (!string.IsNullOrEmpty(duel.GameServer?.ExternalServerId))
                    {
                        await _dathostService.StopServerAsync(duel.GameServer.ExternalServerId);
                    }

                    _logger.LogInformation("Duel completed: {DuelId}, Winner: {WinnerId}, Score: {Team1}-{Team2}", duel.Id, winner.UserId, team1Score, team2Score);
                }
            }

            return new ApiResponse<bool> { Success = true, Data = true };
        }

        private async Task<ApiResponse<bool>> ProcessTournamentMatchResultAsync(TournamentMatch match, DathostMatchWebhook payload, CancellationToken cancellationToken)
        {
            var winnerTeam = DetermineWinnerTeam(payload);
            var team1Score = payload.Team1?.Stats?.Score ?? 0;
            var team2Score = payload.Team2?.Stats?.Score ?? 0;

            if (payload.Finished && winnerTeam != "draw")
            {
                var winnerId = winnerTeam == "team1" ? match.Team1Id : match.Team2Id;

                if (winnerId.HasValue)
                {
                    await _matchRepository.CompleteMatchAsync(match.Id, winnerId.Value, team1Score, team2Score, cancellationToken);
                    _logger.LogInformation("Tournament match completed: {MatchId}, Winner: {WinnerId}, Score: {Team1}-{Team2}", match.Id, winnerId.Value, team1Score, team2Score);
                }
            }

            return new ApiResponse<bool> { Success = true, Data = true };
        }

        private async Task UpdateDuelParticipantStats(Duel duel, List<DathostPlayerWebhook> players, CancellationToken cancellationToken)
        {
            foreach (var player in players)
            {
                var participant = duel.Participants.FirstOrDefault(p => p.User.SteamId == player.SteamId64);
                if (participant != null && player.Stats != null)
                {
                    await _duelRepository.UpdateParticipantStatsAsync(
                        participant.Id,
                        player.Stats.Kills,
                        player.Stats.Deaths,
                        player.Stats.Assists,
                        player.Stats.Score,
                        cancellationToken);
                }
            }
        }

        private string DetermineWinnerTeam(DathostMatchWebhook payload)
        {
            var team1Score = payload.Team1?.Stats?.Score ?? 0;
            var team2Score = payload.Team2?.Stats?.Score ?? 0;

            if (team1Score > team2Score) return "team1";
            if (team2Score > team1Score) return "team2";
            return "draw";
        }
    }
}