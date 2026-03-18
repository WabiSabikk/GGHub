using GGHubBot.Features.Common;
using GGHubBot.Features.Duels;
using GGHubBot.Models;
using GGHubBot.Enums;
using GGHubShared.Enums;
using GGHubShared.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GGHubBot.Features.Payments
{
    public class PaymentCallbackHandler : ICallbackCommand
    {
        private readonly PaymentService _paymentService;
        private readonly PaymentUIService _paymentUIService;
        private readonly DuelUIService _duelUIService;
        private readonly ITelegramBotClient _botClient;
        private readonly IDbContextFactory<BotDbContext> _contextFactory;

        public PaymentCallbackHandler(
            PaymentService paymentService,
            PaymentUIService paymentUIService,
            DuelUIService duelUIService,
            ITelegramBotClient botClient,
            IDbContextFactory<BotDbContext> contextFactory)
        {
            _paymentService = paymentService;
            _paymentUIService = paymentUIService;
            _duelUIService = duelUIService;
            _botClient = botClient;
            _contextFactory = contextFactory;
        }

        public bool CanHandle(string callbackData) => callbackData.StartsWith("payment_");

        public async Task HandleAsync(CallbackQuery callbackQuery, UserState userState, CancellationToken cancellationToken)
        {
            var chatId = callbackQuery.Message!.Chat.Id;
            var data = callbackQuery.Data!;

            try
            {
                switch (data)
                {
                    case var d when d.StartsWith("payment_balance_"):
                        await HandleBalancePaymentAsync(chatId, userState, d, cancellationToken);
                        break;
                    case var d when d.StartsWith("payment_crypto_"):
                        await HandleCryptoPaymentAsync(callbackQuery, userState, d, cancellationToken);
                        break;
                    case var d when d.StartsWith("payment_check_"):
                        await HandleCheckPaymentAsync(chatId, userState, d, cancellationToken);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Payment callback error: {Data}", data);
                await _paymentUIService.ShowPaymentErrorAsync(chatId, "Error", userState.Language, cancellationToken);
            }
        }

        private Task HandleBalancePaymentAsync(long chatId, UserState userState, string data, CancellationToken cancellationToken)
        {
            // In this demo only crypto payments are implemented
            return Task.CompletedTask;
        }

        private async Task HandleCryptoPaymentAsync(CallbackQuery callbackQuery, UserState userState, string data, CancellationToken cancellationToken)
        {
            if (!userState.UserId.HasValue || !Guid.TryParse(data.Split('_')[2], out var duelId))
                return;

            var chatId = callbackQuery.Message!.Chat.Id;
            var messageId = callbackQuery.Message.MessageId;

            var result = await _paymentService.ProcessDuelPaymentAsync(duelId, userState.UserId.Value, PaymentProvider.Cryptomus);

            if (result?.Success == true && result.Data != null)
            {
                await _botClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null, cancellationToken: cancellationToken);

                if (!string.IsNullOrEmpty(result.Message))
                {
                    string url = result.Message;
                    var expired = false;
                    if (result.Message.StartsWith("expired|"))
                    {
                        expired = true;
                        url = result.Message.Substring("expired|".Length);
                    }
                    if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    {
                        var paymentId = PaymentService.ExtractPaymentId(url);
                        await _paymentUIService.ShowCryptomusPaymentAsync(chatId, url, paymentId, userState.Language, expired, cancellationToken);
                        return;
                    }
                }
                await _paymentUIService.ShowPaymentSuccessAsync(chatId, userState.Language, cancellationToken);

                var allPaid = result.Data.Participants.All(p => p.HasPaid);
                if (allPaid)
                {
                    await _paymentUIService.ShowBothDepositsPaidAsync(chatId, userState.Language, cancellationToken);
                    await _duelUIService.ShowReadyPromptAsync(chatId, result.Data.Id, userState.Language, cancellationToken);
                    await NotifyReadyAsync(result.Data, userState.UserId.Value, cancellationToken);
                }
                else
                {
                    await _paymentUIService.ShowWaitingOpponentAsync(chatId, userState.Language, cancellationToken);
                }
            }
            else
            {
                var msg = !string.IsNullOrEmpty(result?.Message) ? result!.Message : string.Join(", ", result?.Errors ?? new List<string>());
                await _paymentUIService.ShowPaymentErrorAsync(chatId, msg, userState.Language, cancellationToken);
            }
        }

        private async Task HandleCheckPaymentAsync(long chatId, UserState userState, string data, CancellationToken cancellationToken)
        {
            var paymentId = data.Split('_')[2];
            var result = await _paymentService.CheckPaymentStatusAsync(paymentId);
            if (result?.Success == true)
            {
                var status = result.Status ?? "unknown";
                if (string.Equals(status, "paid", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(status, "paid_over", StringComparison.OrdinalIgnoreCase))
                {
                    await _paymentUIService.ShowPaymentSuccessAsync(chatId, userState.Language, cancellationToken);
                    // The duel might still be waiting for an opponent or server setup
                    // so we optionally display the waiting prompt if no further data is provided
                    await _paymentUIService.ShowWaitingOpponentAsync(chatId, userState.Language, cancellationToken);
                }
                else
                {
                    await _paymentUIService.ShowPaymentStatusAsync(chatId, status, paymentId, userState.Language, cancellationToken);
                }
            }
            else
            {
                await _paymentUIService.ShowPaymentErrorAsync(chatId, "Payment check failed", userState.Language, cancellationToken);
            }
        }

        private async Task NotifyReadyAsync(DuelDto duel, Guid payerId, CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            foreach (var participant in duel.Participants)
            {
                if (participant.User.Id == payerId)
                    continue;

                var state = await context.UserStates.FirstOrDefaultAsync(u => u.UserId == participant.User.Id, cancellationToken);
                if (state == null)
                    continue;

                var exists = await context.DuelServerNotifications.AnyAsync(n => n.DuelId == duel.Id && n.TelegramId == state.TelegramId && n.Type == DuelServerNotificationType.ReadyPrompt, cancellationToken);
                if (exists)
                    continue;

                await _duelUIService.ShowReadyPromptAsync(state.TelegramId, duel.Id, state.Language, cancellationToken);

                context.DuelServerNotifications.Add(new DuelServerNotification
                {
                    DuelId = duel.Id,
                    TelegramId = state.TelegramId,
                    Type = DuelServerNotificationType.ReadyPrompt,
                    SentAt = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
