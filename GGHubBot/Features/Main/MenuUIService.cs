using GGHubBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace GGHubBot.Features.Main;

public class MenuUIService
{
    private readonly ITelegramBotClient _botClient;
    private readonly LocalizationService _localizationService;

    public MenuUIService(ITelegramBotClient botClient, LocalizationService localizationService)
    {
        _botClient = botClient;
        _localizationService = localizationService;
    }

    public InlineKeyboardMarkup BuildMainMenuKeyboard(string language)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("duels", language), "menu_duels"),
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("tournaments", language), "menu_tournaments")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("profile", language), "menu_profile"),
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("wallet", language), "menu_wallet")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("settings", language), "menu_settings"),
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("help", language), "menu_help")
            }
        });
    }

    public Task ShowMainMenuAsync(long chatId, string language, CancellationToken cancellationToken)
    {
        var keyboard = BuildMainMenuKeyboard(language);
        return _botClient.SendMessage(
            chatId,
            _localizationService.GetText("main_menu", language),
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }
}
