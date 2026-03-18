using GGHubBot.Features.Duels;
using GGHubBot.Features.Payments;
using GGHubBot.Models;
using GGHubBot.Enums;
using GGHubShared.Enums;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using GGHubShared.Models;

namespace GGHubBot.Services
{
    public class DuelHubClientService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IDbContextFactory<BotDbContext> _contextFactory;
        private readonly ApiService _apiService;
        private readonly DuelUIService _duelUIService;
        private readonly PaymentUIService _paymentUIService;
        private HubConnection? _connection;

        public DuelHubClientService(
            IConfiguration configuration,
            IDbContextFactory<BotDbContext> contextFactory,
            ApiService apiService,
            DuelUIService duelUIService,
            PaymentUIService paymentUIService)
        {
            _configuration = configuration;
            _contextFactory = contextFactory;
            _apiService = apiService;
            _duelUIService = duelUIService;
            _paymentUIService = paymentUIService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var apiBase = _configuration["GGHubApi:BaseUrl"] ?? string.Empty;
            var apiRoot = "https://localhost:7277/";//apiBase.EndsWith("/api/") ? apiBase[..^4] : apiBase;
            var hubUrl = new Uri(new Uri(apiRoot), "duel-hub");

            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl + "?clientType=telegram-bot")
                .WithAutomaticReconnect()
                .Build();

            _connection.On<Guid, string, int, string?>("ServerCreated", async (duelId, ip, port, password) =>
            {
                await HandleServerCreatedAsync(duelId, ip, port, password, stoppingToken);
            });

            _connection.On<Guid, Guid>("EntryFeePaid", async (duelId, userId) =>
            {
                await HandleEntryFeePaidAsync(duelId, userId, stoppingToken);
            });

            _connection.On<Guid, Guid>("OpponentJoined", async (duelId, userId) =>
            {
                await HandleOpponentJoinedAsync(duelId, userId, stoppingToken);
            });

            _connection.On<Guid>("ServerStarting", async (duelId) =>
            {
                await HandleServerStartingAsync(duelId, stoppingToken);
            });

            _connection.On<Guid, Guid, DuelForfeitResult>("DuelForfeited", async (duelId, forfeitedUserId, result) =>
            {
                await HandleDuelForfeitedAsync(duelId, forfeitedUserId, result, stoppingToken);
            });

            _connection.Reconnected += async (connectionId) =>
            {
                Log.Information("Reconnected to DuelHub: {ConnectionId}", connectionId);
                var duels = await GetActiveDuelIdsAsync(stoppingToken);
                foreach (var duel in duels)
                {
                    await _connection.InvokeAsync("JoinDuelGroup", duel.ToString(), stoppingToken);
                }
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _connection.StartAsync(stoppingToken);
                    break;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to connect to DuelHub. Retrying...");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task HandleServerCreatedAsync(Guid duelId, string ip, int port, string? password, CancellationToken cancellationToken)
        {
            try
            {
                var duelResp = await _apiService.GetFullDuelAsync(duelId);
                if (duelResp?.Success != true || duelResp.Data == null)
                    return;

                await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

                foreach (var participant in duelResp.Data.Participants)
                {
                    var state = await context.UserStates.FirstOrDefaultAsync(u => u.UserId == participant.User.Id, cancellationToken);
                    if (state == null)
                        continue;

                    var exists = await context.DuelServerNotifications.AnyAsync(n => n.DuelId == duelId && n.TelegramId == state.TelegramId && n.Type == DuelServerNotificationType.ServerInfo, cancellationToken);
                    if (exists)
                        continue;

                    duelResp.Data.GameServer = new GGHubShared.Models.GameServerDto
                    {
                        ServerIp = ip,
                        ServerPort = port,
                        Password = password
                    };
                    await _duelUIService.ShowGetServerAsync(state.TelegramId, duelResp.Data, state.Language, cancellationToken);

                    context.DuelServerNotifications.Add(new DuelServerNotification
                    {
                        DuelId = duelId,
                        TelegramId = state.TelegramId,
                        Type = DuelServerNotificationType.ServerInfo,
                        SentAt = DateTime.UtcNow
                    });
                }

                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling ServerCreated for duel {DuelId}", duelId);
            }
        }

        private async Task HandleEntryFeePaidAsync(Guid duelId, Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                var duelResp = await _apiService.GetFullDuelAsync(duelId);
                if (duelResp?.Success != true || duelResp.Data == null)
                    return;

                await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

                var payerState = await context.UserStates.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
                if (payerState != null)
                {
                    await _paymentUIService.ShowPaymentSuccessAsync(payerState.TelegramId, payerState.Language, cancellationToken);
                }

                var allPaid = duelResp.Data.Participants.All(p => p.HasPaid);

                if (allPaid)
                {
                    foreach (var participant in duelResp.Data.Participants)
                    {
                        var state = await context.UserStates.FirstOrDefaultAsync(u => u.UserId == participant.User.Id, cancellationToken);
                        if (state == null)
                            continue;

                        await _paymentUIService.ShowBothDepositsPaidAsync(state.TelegramId, state.Language, cancellationToken);
                        await _duelUIService.ShowReadyPromptAsync(state.TelegramId, duelId, state.Language, cancellationToken);

                        var exists = await context.DuelServerNotifications.AnyAsync(n => n.DuelId == duelId && n.TelegramId == state.TelegramId && n.Type == DuelServerNotificationType.ReadyPrompt, cancellationToken);
                        if (!exists)
                        {
                            context.DuelServerNotifications.Add(new DuelServerNotification
                            {
                                DuelId = duelId,
                                TelegramId = state.TelegramId,
                                Type = DuelServerNotificationType.ReadyPrompt,
                                SentAt = DateTime.UtcNow
                            });
                        }
                    }

                    await context.SaveChangesAsync(cancellationToken);
                }
                else if (payerState != null)
                {
                    await _paymentUIService.ShowWaitingOpponentAsync(payerState.TelegramId, payerState.Language, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling EntryFeePaid for duel {DuelId}", duelId);
            }
        }

        private async Task HandleOpponentJoinedAsync(Guid duelId, Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                var duelResp = await _apiService.GetFullDuelAsync(duelId);
                if (duelResp?.Success != true || duelResp.Data == null)
                    return;

                await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

                foreach (var participant in duelResp.Data.Participants.Where(p => p.User.Id != userId))
                {
                    var state = await context.UserStates.FirstOrDefaultAsync(u => u.UserId == participant.User.Id, cancellationToken);
                    if (state == null)
                        continue;

                    await _duelUIService.ShowSuccessAsync(state.TelegramId, "opponent_joined", state.Language, cancellationToken);
                    var duels = await _apiService.GetUserDuelsAsync(participant.User.Id);
                    if (duels?.Success == true && duels.Data != null)
                    {
                        await _duelUIService.ShowMyDuelsAsync(state.TelegramId, duels.Data, participant.User.Id, state.Language, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling OpponentJoined for duel {DuelId}", duelId);
            }
        }
        private async Task HandleServerStartingAsync(Guid duelId, CancellationToken cancellationToken)
        {
            try
            {
                var duelResp = await _apiService.GetFullDuelAsync(duelId);
                if (duelResp?.Success != true || duelResp.Data == null)
                    return;

                await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

                foreach (var participant in duelResp.Data.Participants)
                {
                    var state = await context.UserStates.FirstOrDefaultAsync(u => u.UserId == participant.User.Id, cancellationToken);
                    if (state == null)
                        continue;

                    await _duelUIService.ShowServerStartingAsync(state.TelegramId, state.Language, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling ServerStarting for duel {DuelId}", duelId);
            }
        }

        private async Task HandleDuelForfeitedAsync(Guid duelId, Guid forfeitedUserId, DuelForfeitResult result, CancellationToken cancellationToken)
        {
            try
            {
                var duelResp = await _apiService.GetFullDuelAsync(duelId);
                if (duelResp?.Success != true || duelResp.Data == null)
                    return;

                await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

                foreach (var participant in duelResp.Data.Participants.Where(p => p.User.Id != forfeitedUserId))
                {
                    var state = await context.UserStates.FirstOrDefaultAsync(u => u.UserId == participant.User.Id, cancellationToken);
                    if (state == null)
                        continue;

                    if (result.RefundIssued)
                    {
                        await _duelUIService.ShowOpponentForfeitedRefundAsync(state.TelegramId, result, state.Language, cancellationToken);
                    }
                    else
                    {
                        await _duelUIService.ShowOpponentForfeitedWinAsync(state.TelegramId, result, state.Language, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling DuelForfeited for duel {DuelId}", duelId);
            }
        }
        private async Task<List<Guid>> GetActiveDuelIdsAsync(CancellationToken cancellationToken)
        {
            var result = new List<Guid>();

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var userIds = context.UserStates
                .Where(s => s.UserId.HasValue)
                .Select(s => s.UserId!.Value)
                .Distinct()
                .ToList();

            foreach (var userId in userIds)
            {
                var duels = await _apiService.GetUserDuelsAsync(userId);
                if (duels?.Success == true && duels.Data != null)
                {
                    result.AddRange(duels.Data
                        .Where(d => d.Status == DuelStatus.WaitingForPlayers ||
                                    d.Status == DuelStatus.PaymentPending ||
                                    d.Status == DuelStatus.Starting ||
                                    d.Status == DuelStatus.InProgress)
                        .Select(d => d.Id));
                }
            }

            return result.Distinct().ToList();
        }
    }
}
