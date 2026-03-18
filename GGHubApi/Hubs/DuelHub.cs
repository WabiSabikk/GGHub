using GGHubApi.Services;
using GGHubShared.Models;
using Microsoft.AspNetCore.SignalR;

namespace GGHubApi.Hubs
{
    public class DuelHub : Hub
    {
        private readonly ILogger<DuelHub> _logger;
        private readonly IConnectionManager _connectionManager;

        public DuelHub(ILogger<DuelHub> logger, IConnectionManager connectionManager)
        {
            _logger = logger;
            _connectionManager = connectionManager;
        }


        public async Task JoinDuelGroup(string duelId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Duel_{duelId}");
            _logger.LogInformation("User {ConnectionId} joined duel group: {DuelId}", Context.ConnectionId, duelId);
        }

        public async Task LeaveDuelGroup(string duelId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Duel_{duelId}");
            _logger.LogInformation("User {ConnectionId} left duel group: {DuelId}", Context.ConnectionId, duelId);
        }


        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var clientType = httpContext?.Request.Query["clientType"].ToString();

            if (clientType == "telegram-bot")
            {
                _connectionManager.RegisterBotConnection(Context.ConnectionId);
                _logger.LogInformation("Telegram bot connected: {ConnectionId}", Context.ConnectionId);
            }
            else
            {
                long userId = 0;
                if (httpContext != null && httpContext.Request.Query.TryGetValue("userId", out var idValue))
                {
                    long.TryParse(idValue, out userId);
                }

                _connectionManager.AddUserConnection(Context.ConnectionId, userId);
                _logger.LogInformation("Connection {ConnectionId} mapped to user {UserId}", Context.ConnectionId, userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _connectionManager.RemoveConnection(Context.ConnectionId);
            _logger.LogInformation("Connection {ConnectionId} disconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }

    public interface IDuelHubService
    {
        Task AddUserToDuelGroup(Guid duelId, Guid userId);
        Task NotifyServerCreated(Guid duelId, GameServerDto server);
        Task NotifyEntryFeePaid(Guid duelId, Guid userId, IEnumerable<Guid> participantUserIds);
        Task NotifyOpponentJoined(Guid duelId, Guid userId, IEnumerable<Guid> participantUserIds);
        Task NotifyRoundEndAsync(Guid duelId, int team1Score, int team2Score);
        Task NotifyServerStarting(Guid duelId);
        Task NotifyDuelForfeited(Guid duelId, Guid forfeitedUserId, Guid opponentUserId, DuelForfeitResult result);
    }

    public class DuelHubService : IDuelHubService
    {
        private readonly IHubContext<DuelHub> _hubContext;
        private readonly ILogger<DuelHubService> _logger;
        private readonly IConnectionManager _connectionManager;
        private readonly IUserMappingService _userMappingService;

        public DuelHubService(IHubContext<DuelHub> hubContext,
            ILogger<DuelHubService> logger,
            IConnectionManager connectionManager,
            IUserMappingService userMappingService)
        {
            _hubContext = hubContext;
            _logger = logger;
            _connectionManager = connectionManager;
            _userMappingService = userMappingService;
        }

   

        public async Task AddUserToDuelGroup(Guid duelId, Guid userId)
        {
            try
            {
                var telegramIds = await _userMappingService.GetTelegramIdsAsync([userId]);
                var connections = _connectionManager.GetConnectionsForUsers(telegramIds);
                if (!connections.Any())
                {
                    connections = _connectionManager.GetBotConnectionIds().ToList();
                }

                foreach (var connection in connections)
                {
                    await _hubContext.Groups.AddToGroupAsync(connection, $"Duel_{duelId}");
                }
                _logger.LogInformation("Added user {UserId} to duel group {DuelId}", userId, duelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add user {UserId} to duel group {DuelId}", userId, duelId);
            }
        }

        public async Task NotifyServerCreated(Guid duelId, GameServerDto server)
        {
            try
            {
                await _hubContext.Clients.Group($"Duel_{duelId}")
        .SendAsync("ServerCreated", duelId, server.ServerIp, server.ServerPort, server.Password);

                _logger.LogInformation("Notified server created for duel: {DuelId}", duelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying server created: {DuelId}", duelId);
            }
        }

        public async Task NotifyEntryFeePaid(Guid duelId, Guid userId, IEnumerable<Guid> participantUserIds)
        {
            try
            {
                var telegramIds = await _userMappingService.GetTelegramIdsAsync(participantUserIds.ToArray());
                var connections = _connectionManager.GetConnectionsForUsers(telegramIds);
                if (!connections.Any())
                {
                    connections = _connectionManager.GetBotConnectionIds().ToList();
                }

                if (connections.Any())
                {
                    await _hubContext.Clients.Clients(connections)
                        .SendAsync("EntryFeePaid", duelId, userId);
                }

                _logger.LogInformation("Notified entry fee paid for duel: {DuelId}, user: {UserId}", duelId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying entry fee paid: {DuelId}", duelId);
            }
        }

        public async Task NotifyOpponentJoined(Guid duelId, Guid userId, IEnumerable<Guid> participantUserIds)
        {
            try
            {
                var telegramIds = await _userMappingService.GetTelegramIdsAsync(participantUserIds.Where(id => id != userId).ToArray());
                var connections = _connectionManager.GetConnectionsForUsers(telegramIds);
                if (!connections.Any())
                {
                    connections = _connectionManager.GetBotConnectionIds().ToList();
                }

                if (connections.Any())
                {
                    await _hubContext.Clients.Clients(connections)
                        .SendAsync("OpponentJoined", duelId, userId);
                }

                _logger.LogInformation("Notified opponent joined duel: {DuelId}, user: {UserId}", duelId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying opponent joined: {DuelId}", duelId);
            }
        }

        public async Task NotifyRoundEndAsync(Guid duelId, int team1Score, int team2Score)
        {
            try
            {
                await _hubContext.Clients.Group($"Duel_{duelId}")
                    .SendAsync("RoundEnd", new { team1Score, team2Score });

                _logger.LogInformation("Notified round end for duel: {DuelId}", duelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying round end: {DuelId}", duelId);
            }
        }
        public async Task NotifyServerStarting(Guid duelId)
        {
            try
            {
                await _hubContext.Clients.Group($"Duel_{duelId}")
                    .SendAsync("ServerStarting", duelId);

                _logger.LogInformation("Notified server starting for duel: {DuelId}", duelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying server starting: {DuelId}", duelId);
            }
        }

        public async Task NotifyDuelForfeited(Guid duelId, Guid forfeitedUserId, Guid opponentUserId, DuelForfeitResult result)
        {
            try
            {
                var telegramIds = await _userMappingService.GetTelegramIdsAsync(new[] { opponentUserId });
                var connections = _connectionManager.GetConnectionsForUsers(telegramIds);

                if (!connections.Any())
                {
                    connections = _connectionManager.GetBotConnectionIds().ToList();
                }

                if (connections.Any())
                {
                    await _hubContext.Clients.Clients(connections)
                        .SendAsync("DuelForfeited", duelId, forfeitedUserId, result);
                }

                _logger.LogInformation("Notified duel forfeited: {DuelId}, user: {UserId}, refund: {Refund}",
                    duelId, forfeitedUserId, result.RefundIssued);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying duel forfeited: {DuelId}", duelId);
            }
        }
    }
}
