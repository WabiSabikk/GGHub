using Microsoft.AspNetCore.SignalR;
using GGHubShared.Models;
using GGHubShared.Enums;

namespace GGHubApi.Hubs
{
    public class TournamentHub : Hub
    {
        private readonly ILogger<TournamentHub> _logger;

        public TournamentHub(ILogger<TournamentHub> logger)
        {
            _logger = logger;
        }

        public async Task JoinTournamentGroup(string tournamentId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Tournament_{tournamentId}");
            _logger.LogInformation("User {ConnectionId} joined tournament group: {TournamentId}", Context.ConnectionId, tournamentId);
        }

        public async Task LeaveTournamentGroup(string tournamentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Tournament_{tournamentId}");
            _logger.LogInformation("User {ConnectionId} left tournament group: {TournamentId}", Context.ConnectionId, tournamentId);
        }

        public async Task JoinMatchGroup(string matchId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Match_{matchId}");
            _logger.LogInformation("User {ConnectionId} joined match group: {MatchId}", Context.ConnectionId, matchId);
        }

        public async Task LeaveMatchGroup(string matchId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Match_{matchId}");
            _logger.LogInformation("User {ConnectionId} left match group: {MatchId}", Context.ConnectionId, matchId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("User {ConnectionId} disconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }

    public interface ITournamentHubService
    {
        Task NotifyTournamentUpdated(Guid tournamentId, TournamentDto tournament);
        Task NotifyTeamJoined(Guid tournamentId, TournamentTeamDto team);
        Task NotifyTeamLeft(Guid tournamentId, Guid teamId);
        Task NotifyPaymentCompleted(Guid tournamentId, Guid teamId);
        Task NotifyTournamentStarted(Guid tournamentId);
        Task NotifyMatchStarted(Guid tournamentId, Guid matchId, TournamentMatchDto match);
        Task NotifyMatchCompleted(Guid tournamentId, Guid matchId, TournamentMatchDto match);
        Task NotifyRoundCompleted(Guid tournamentId, int round);
        Task NotifyTournamentCompleted(Guid tournamentId, Guid winnerId);
        Task NotifyServerReady(Guid matchId, string serverIp, int port, string password);
        Task NotifyMatchStatusChanged(Guid matchId, TournamentMatchStatus status);
        Task NotifyRoundEndAsync(Guid matchId, int team1Score, int team2Score);
    }

    public class TournamentHubService : ITournamentHubService
    {
        private readonly IHubContext<TournamentHub> _hubContext;
        private readonly ILogger<TournamentHubService> _logger;

        public TournamentHubService(IHubContext<TournamentHub> hubContext, ILogger<TournamentHubService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyTournamentUpdated(Guid tournamentId, TournamentDto tournament)
        {
            try
            {
                await _hubContext.Clients.Group($"Tournament_{tournamentId}")
                    .SendAsync("TournamentUpdated", tournament);

                _logger.LogInformation("Notified tournament updated: {TournamentId}", tournamentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying tournament updated: {TournamentId}", tournamentId);
            }
        }

        public async Task NotifyTeamJoined(Guid tournamentId, TournamentTeamDto team)
        {
            try
            {
                await _hubContext.Clients.Group($"Tournament_{tournamentId}")
                    .SendAsync("TeamJoined", team);

                _logger.LogInformation("Notified team joined tournament: {TournamentId}, Team: {TeamId}", tournamentId, team.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying team joined: {TournamentId}", tournamentId);
            }
        }

        public async Task NotifyTeamLeft(Guid tournamentId, Guid teamId)
        {
            try
            {
                await _hubContext.Clients.Group($"Tournament_{tournamentId}")
                    .SendAsync("TeamLeft", teamId);

                _logger.LogInformation("Notified team left tournament: {TournamentId}, Team: {TeamId}", tournamentId, teamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying team left: {TournamentId}", tournamentId);
            }
        }

        public async Task NotifyPaymentCompleted(Guid tournamentId, Guid teamId)
        {
            try
            {
                await _hubContext.Clients.Group($"Tournament_{tournamentId}")
                    .SendAsync("PaymentCompleted", teamId);

                _logger.LogInformation("Notified payment completed: {TournamentId}, Team: {TeamId}", tournamentId, teamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying payment completed: {TournamentId}", tournamentId);
            }
        }

        public async Task NotifyTournamentStarted(Guid tournamentId)
        {
            try
            {
                await _hubContext.Clients.Group($"Tournament_{tournamentId}")
                    .SendAsync("TournamentStarted", tournamentId);

                _logger.LogInformation("Notified tournament started: {TournamentId}", tournamentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying tournament started: {TournamentId}", tournamentId);
            }
        }

        public async Task NotifyMatchStarted(Guid tournamentId, Guid matchId, TournamentMatchDto match)
        {
            try
            {
                await _hubContext.Clients.Group($"Tournament_{tournamentId}")
                    .SendAsync("MatchStarted", match);

                await _hubContext.Clients.Group($"Match_{matchId}")
                    .SendAsync("MatchStarted", match);

                _logger.LogInformation("Notified match started: {TournamentId}, Match: {MatchId}", tournamentId, matchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying match started: {TournamentId}", tournamentId);
            }
        }

        public async Task NotifyMatchCompleted(Guid tournamentId, Guid matchId, TournamentMatchDto match)
        {
            try
            {
                await _hubContext.Clients.Group($"Tournament_{tournamentId}")
                    .SendAsync("MatchCompleted", match);

                await _hubContext.Clients.Group($"Match_{matchId}")
                    .SendAsync("MatchCompleted", match);

                _logger.LogInformation("Notified match completed: {TournamentId}, Match: {MatchId}", tournamentId, matchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying match completed: {TournamentId}", tournamentId);
            }
        }

        public async Task NotifyRoundCompleted(Guid tournamentId, int round)
        {
            try
            {
                await _hubContext.Clients.Group($"Tournament_{tournamentId}")
                    .SendAsync("RoundCompleted", round);

                _logger.LogInformation("Notified round completed: {TournamentId}, Round: {Round}", tournamentId, round);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying round completed: {TournamentId}", tournamentId);
            }
        }

        public async Task NotifyTournamentCompleted(Guid tournamentId, Guid winnerId)
        {
            try
            {
                await _hubContext.Clients.Group($"Tournament_{tournamentId}")
                    .SendAsync("TournamentCompleted", winnerId);

                _logger.LogInformation("Notified tournament completed: {TournamentId}, Winner: {WinnerId}", tournamentId, winnerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying tournament completed: {TournamentId}", tournamentId);
            }
        }

        public async Task NotifyServerReady(Guid matchId, string serverIp, int port, string password)
        {
            try
            {
                await _hubContext.Clients.Group($"Match_{matchId}")
                    .SendAsync("ServerReady", new { serverIp, port, password });

                _logger.LogInformation("Notified server ready for match: {MatchId}", matchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying server ready: {MatchId}", matchId);
            }
        }

        public async Task NotifyMatchStatusChanged(Guid matchId, TournamentMatchStatus status)
        {
            try
            {
                await _hubContext.Clients.Group($"Match_{matchId}")
                    .SendAsync("MatchStatusChanged", status);

                _logger.LogInformation("Notified match status changed: {MatchId}, Status: {Status}", matchId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying match status changed: {MatchId}", matchId);
            }
        }

        public async Task NotifyRoundEndAsync(Guid matchId, int team1Score, int team2Score)
        {
            try
            {
                await _hubContext.Clients.Group($"Match_{matchId}")
                    .SendAsync("RoundEnd", new { team1Score, team2Score });

                _logger.LogInformation("Notified round end for match: {MatchId}", matchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying round end: {MatchId}", matchId);
            }
        }
    }
}