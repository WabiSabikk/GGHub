using GGHubBot.Features.Main;
using GGHubBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace GGHubBot.Features.Payments
{
    public class PaymentUIService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly LocalizationService _localizationService;
        private readonly MenuUIService _menuUIService;
        public PaymentUIService(ITelegramBotClient botClient, LocalizationService localizationService, MenuUIService menuUIService)
        {
            _botClient = botClient;
            _localizationService = localizationService;
            _menuUIService = menuUIService;
        }

        public async Task ShowCryptomusPaymentAsync(long chatId, string paymentUrl, string paymentId, string language, bool isExpired, CancellationToken cancellationToken)
        {
            if (isExpired)
            {
                await _botClient.SendMessage(chatId, _localizationService.GetText("payment_expired", language), cancellationToken: cancellationToken);
            }

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new [] { InlineKeyboardButton.WithUrl(_localizationService.GetText("pay", language), paymentUrl) },
                new [] { InlineKeyboardButton.WithCallbackData(_localizationService.GetText("check_payment", language), $"payment_check_{paymentId}") }
            });

            await _botClient.SendMessage(chatId, _localizationService.GetText("pay_cryptomus", language), replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        public async Task ShowPaymentSuccessAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(chatId, _localizationService.GetText("entry_fee_paid", language), cancellationToken: cancellationToken);
        }

        public async Task ShowBothDepositsPaidAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(chatId, _localizationService.GetText("both_deposits_paid", language), cancellationToken: cancellationToken);
        }

        public async Task ShowWaitingOpponentAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            var keyboard = _menuUIService.BuildMainMenuKeyboard(language);
            await _botClient.SendMessage(
                chatId,
                _localizationService.GetText("waiting_opponent", language),
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }

        public async Task ShowWaitingOpponentReadyAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(chatId, _localizationService.GetText("waiting_opponent_ready", language), cancellationToken: cancellationToken);
        }

        public async Task ShowPaymentStatusAsync(long chatId, string status, string paymentId, string language, CancellationToken cancellationToken)
        {
            var text = string.Format(_localizationService.GetText("payment_status", language), status);
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData(_localizationService.GetText("check_payment", language), $"payment_check_{paymentId}") }
            });

            await _botClient.SendMessage(chatId, text, replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        public async Task ShowPaymentErrorAsync(long chatId, string message, string language, CancellationToken cancellationToken)
        {
            var errorText = $"{_localizationService.GetText("error", language)}: {message}";
            await _botClient.SendMessage(chatId, errorText, cancellationToken: cancellationToken);
        }
    }
}
