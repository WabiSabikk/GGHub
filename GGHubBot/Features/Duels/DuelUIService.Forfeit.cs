using GGHubShared.Models;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

namespace GGHubBot.Features.Duels
{
    public partial class DuelUIService
    {
        public async Task ShowForfeitConfirmationAsync(long chatId, Guid duelId, string language, CancellationToken ct)
        {
            var text = language == "uk"
                ? "⚠️ Ви впевнені що хочете покинути матч?\n\n" +
                  "Оберіть причину:"
                : "⚠️ Are you sure you want to forfeit the match?\n\n" +
                  "Choose reason:";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        language == "uk" ? "✅ Так, підтвердити" : "✅ Yes, confirm",
                        $"duel_forfeit_confirm_{duelId}")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        language == "uk" ? "🌐 Поганий пінг" : "🌐 Bad ping",
                        $"duel_forfeit_badping_{duelId}")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        language == "uk" ? "❌ Ні, повернутися" : "❌ No, go back",
                        "duel_cancel_forfeit")
                }
            });

            await _botClient.SendMessage(chatId, text, replyMarkup: keyboard, cancellationToken: ct);
        }

        public async Task ShowPingCheckingAsync(long chatId, string language, CancellationToken ct)
        {
            var text = language == "uk"
                ? "🔍 Перевіряємо пінг..."
                : "🔍 Checking ping...";

            await _botClient.SendMessage(chatId, text, cancellationToken: ct);
        }

        public async Task ShowForfeitSuccessAsync(long chatId, DuelForfeitResult result, string language, CancellationToken ct)
        {
            var text = language == "uk"
                ? $"✅ Ви покинули матч.\n\n{result.Message}"
                : $"✅ You forfeited the match.\n\n{result.Message}";

            await _botClient.SendMessage(chatId, text, cancellationToken: ct);
        }

        public async Task ShowForfeitBadPingRefundAsync(long chatId, DuelForfeitResult result, string language, CancellationToken ct)
        {
            var text = language == "uk"
                ? $"✅ Підтверджено поганий пінг!\n\n" +
                  $"📊 Виміряний пінг: {result.MeasuredPing}ms\n" +
                  $"💰 Кошти повернуті обом гравцям"
                : $"✅ Bad ping confirmed!\n\n" +
                  $"📊 Measured ping: {result.MeasuredPing}ms\n" +
                  $"💰 Funds refunded to both players";

            await _botClient.SendMessage(chatId, text, cancellationToken: ct);
        }

        public async Task ShowForfeitPingNormalDefeatAsync(long chatId, DuelForfeitResult result, string language, CancellationToken ct)
        {
            var text = language == "uk"
                ? $"❌ Пінг в межах норми\n\n" +
                  $"📊 Виміряний пінг: {result.MeasuredPing}ms\n" +
                  $"⚠️ Зараховано технічну поразку"
                : $"❌ Ping is acceptable\n\n" +
                  $"📊 Measured ping: {result.MeasuredPing}ms\n" +
                  $"⚠️ Technical defeat applied";

            await _botClient.SendMessage(chatId, text, cancellationToken: ct);
        }

        public async Task ShowOpponentForfeitedWinAsync(long chatId, DuelForfeitResult result, string language, CancellationToken ct)
        {
            var text = language == "uk"
                ? $"🎉 Ваш опонент покинув матч!\n\n" +
                  $"✅ Вам зараховано перемогу\n" +
                  $"💰 Виграш зараховано на баланс"
                : $"🎉 Your opponent forfeited!\n\n" +
                  $"✅ Victory awarded to you\n" +
                  $"💰 Winnings credited to balance";

            await _botClient.SendMessage(chatId, text, cancellationToken: ct);
        }

        public async Task ShowOpponentForfeitedRefundAsync(long chatId, DuelForfeitResult result, string language, CancellationToken ct)
        {
            var text = language == "uk"
                ? $"⚠️ Ваш опонент покинув матч через поганий пінг\n\n" +
                  $"📊 Виміряний пінг: {result.MeasuredPing}ms\n" +
                  $"💰 Депозит повернуто обом гравцям"
                : $"⚠️ Your opponent forfeited due to bad ping\n\n" +
                  $"📊 Measured ping: {result.MeasuredPing}ms\n" +
                  $"💰 Deposit refunded to both players";

            await _botClient.SendMessage(chatId, text, cancellationToken: ct);
        }
    }
}
