using GGHubBot.Models;
using GGHubBot.Services;
using GGHubBot.Features.Common;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.Extensions.Configuration;
using GGHubBot.Features.Main;

namespace GGHubBot.Handlers
{
    public class CallbackHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserStateService _userStateService;
        private readonly LocalizationService _localizationService;
        private readonly ApiService _apiService;
        private readonly TournamentHandler _tournamentHandler;
        private readonly AdminService _adminService;
        private readonly IConfiguration _configuration;
        private readonly CallbackCommandResolver _commandResolver;
        private readonly MenuUIService _menuUIService;

        public CallbackHandler(
            ITelegramBotClient botClient,
            UserStateService userStateService,
            LocalizationService localizationService,
            ApiService apiService,
            TournamentHandler tournamentHandler,
            AdminService adminService,
            IConfiguration configuration,
            CallbackCommandResolver commandResolver,
            MenuUIService menuUIService)
        {
            _botClient = botClient;
            _userStateService = userStateService;
            _localizationService = localizationService;
            _apiService = apiService;
            _tournamentHandler = tournamentHandler;
            _adminService = adminService;
            _configuration = configuration;
            _commandResolver = commandResolver;
            _menuUIService = menuUIService;
        }

        public async Task HandleAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.From == null || callbackQuery.Data == null)
                return;

            var userState = await _userStateService.GetOrCreateUserStateAsync(callbackQuery.From.Id);
            var lang = userState.Language;
            var chatId = callbackQuery.Message!.Chat.Id;

            if ((!userState.IsAuthenticated || string.IsNullOrEmpty(userState.SteamId)) &&
                RequiresAuth(callbackQuery.Data))
            {
                await ShowAuthRequiredAsync(chatId, lang, cancellationToken);
                return;
            }

            Log.Information("Callback from {UserId}: {Data}, State: {State}",
                callbackQuery.From.Id, callbackQuery.Data, userState.State);

            try
            {
                await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);

                var command = _commandResolver.Resolve(callbackQuery.Data);
                if (command != null)
                {
                    await command.HandleAsync(callbackQuery, userState, cancellationToken);
                    return;
                }

                switch (callbackQuery.Data)
                {
                    case var data when data.StartsWith("lang_"):
                        await HandleLanguageSelectionAsync(callbackQuery, data, cancellationToken);
                        break;

                    case var data when data.StartsWith("menu_"):
                        await HandleMenuNavigationAsync(callbackQuery, userState, data, cancellationToken);
                        break;


                    case var data when data.StartsWith("tournament_"):
                        await _tournamentHandler.HandleCallbackAsync(callbackQuery, userState, data, cancellationToken);
                        break;

                    case var data when data.StartsWith("wallet_"):
                        await HandleWalletCallbackAsync(callbackQuery, userState, data, cancellationToken);
                        break;

                    case var data when data.StartsWith("admin_"):
                        await _adminService.HandleCallbackAsync(callbackQuery, data, cancellationToken);
                        break;

                    case "back_main":
                        await ShowMainMenuAsync(callbackQuery.Message!.Chat.Id, lang, cancellationToken);
                        await _userStateService.UpdateUserStateAsync(callbackQuery.From.Id, BotState.MainMenu);
                        break;

                    default:
                        Log.Warning("Unknown callback data: {Data}", callbackQuery.Data);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling callback: {Data}", callbackQuery.Data);

                await _botClient.SendMessage(
                    callbackQuery.Message!.Chat.Id,
                    _localizationService.GetText("error", lang),
                    cancellationToken: cancellationToken);
            }
        }

        private async Task HandleLanguageSelectionAsync(CallbackQuery callbackQuery, string data, CancellationToken cancellationToken)
        {
            var language = data.Split('_')[1];
            await _userStateService.SetLanguageAsync(callbackQuery.From.Id, language);

            await _botClient.EditMessageText(
                callbackQuery.Message!.Chat.Id,
                callbackQuery.Message.MessageId,
                _localizationService.GetText("language_selected", language),
                cancellationToken: cancellationToken);

            await Task.Delay(1000, cancellationToken);

            await ShowMainMenuAsync(callbackQuery.Message.Chat.Id, language, cancellationToken);
            await _userStateService.UpdateUserStateAsync(callbackQuery.From.Id, BotState.MainMenu);
        }

        private async Task HandleMenuNavigationAsync(CallbackQuery callbackQuery, UserState userState, string data, CancellationToken cancellationToken)
        {
            var lang = userState.Language;
            var chatId = callbackQuery.Message!.Chat.Id;

            if ((!userState.IsAuthenticated || string.IsNullOrEmpty(userState.SteamId)) &&
                !data.Contains("settings") && !data.Contains("help"))
            {
                await ShowAuthRequiredAsync(chatId, lang, cancellationToken);
                return;
            }

            switch (data)
            {
                case "menu_duels":
                    await ShowDuelsMenuAsync(chatId, lang, cancellationToken);
                    await _userStateService.UpdateUserStateAsync(userState.TelegramId, BotState.MainMenu);
                    break;

                case "menu_tournaments":
                    await ShowTournamentsMenuAsync(chatId, lang, cancellationToken);
                    await _userStateService.UpdateUserStateAsync(userState.TelegramId, BotState.MainMenu);
                    break;

                case "menu_profile":
                    await ShowProfileAsync(chatId, userState, cancellationToken);
                    break;

                case "menu_wallet":
                    await ShowWalletAsync(chatId, userState, cancellationToken);
                    break;

                case "menu_settings":
                    await ShowSettingsAsync(chatId, lang, cancellationToken);
                    break;

                case "menu_help":
                    await ShowHelpAsync(chatId, lang, cancellationToken);
                    break;
            }
        }

        private async Task HandleWalletCallbackAsync(CallbackQuery callbackQuery, UserState userState, string data, CancellationToken cancellationToken)
        {
            var chatId = callbackQuery.Message!.Chat.Id;
            var lang = userState.Language;

            switch (data)
            {
                case "wallet_deposit":
                    await _botClient.SendMessage(
                        chatId,
                        _localizationService.GetText("entry_fee", lang),
                        cancellationToken: cancellationToken);

                    await _userStateService.UpdateUserStateAsync(userState.TelegramId, BotState.WalletDeposit,
                        new WalletOperationState { Operation = "deposit" });
                    break;

                case "wallet_withdraw":
                    await _botClient.SendMessage(
                        chatId,
                        "💸 Enter withdraw amount (minimum €10):",
                        cancellationToken: cancellationToken);

                    await _userStateService.UpdateUserStateAsync(userState.TelegramId, BotState.WalletWithdraw,
                        new WalletOperationState { Operation = "withdraw" });
                    break;

                case "wallet_history":
                    await ShowTransactionHistoryAsync(chatId, userState, cancellationToken);
                    break;
            }
        }

        private Task ShowMainMenuAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            return _menuUIService.ShowMainMenuAsync(chatId, language, cancellationToken);
        }

        private async Task ShowDuelsMenuAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("create_duel", language), "duel_create"),
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("find_duel", language), "duel_find")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("my_duels", language), "duel_my"),
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("join_by_code", language), "duel_join_code")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("back", language), "back_main")
            }
        });

            await _botClient.SendMessage(
                chatId,
                _localizationService.GetText("duels", language),
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }

        private async Task ShowTournamentsMenuAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("create_tournament", language), "tournament_create"),
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("available_tournaments", language), "tournament_available")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("my_tournaments", language), "tournament_my"),
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("join_by_code", language), "tournament_join_code")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("back", language), "back_main")
            }
        });

            await _botClient.SendMessage(
                chatId,
                _localizationService.GetText("tournaments", language),
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }

        private async Task ShowAuthRequiredAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            var callbackUrl = _configuration["Steam:RedirectUrl"] ?? string.Empty;
            var authUrl = callbackUrl.Replace("callback", "auth") + $"?chatId={chatId}";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
               InlineKeyboardButton.WithUrl(
                _localizationService.GetText("login_steam", language),
                authUrl)
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("back", language), "back_main")
            }
        });

            await _botClient.SendMessage(
                chatId,
                _localizationService.GetText("auth_required", language),
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }

        private async Task ShowProfileAsync(long chatId, UserState userState, CancellationToken cancellationToken)
        {
            var user = await _apiService.GetUserByTelegramAsync(userState.TelegramId);

            if (user?.Success == true && user.Data != null)
            {
                var profile = user.Data;
                var metrics = await _apiService.GetUserMatchMetricsAsync(profile.Id);
                var text = $"👤 {_localizationService.GetText("profile", userState.Language)}\n\n" +
                          $"🎮 {profile.Username}\n" +
                          $"📈 Rating: {profile.Rating}\n" +
                          $"🏆 Wins: {profile.Wins}\n" +
                          $"💀 Losses: {profile.Losses}\n" +
                          $"💰 Balance: €{profile.Balance:F2}";

                if (metrics?.Success == true && metrics.Data != null)
                    text += $"\n💵 Avg. deposit: €{metrics.Data.AverageDeposit:F2}";

                var keyboard = new InlineKeyboardMarkup(new[]
                {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(_localizationService.GetText("back", userState.Language), "back_main")
                }
            });

                await _botClient.SendMessage(
                    chatId,
                    text,
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);
            }
        }

        private async Task ShowWalletAsync(long chatId, UserState userState, CancellationToken cancellationToken)
        {
            var user = await _apiService.GetUserByTelegramAsync(userState.TelegramId);

            if (user?.Success == true && user.Data != null)
            {
                var balance = user.Data.Balance;
                var text = $"{_localizationService.GetText("wallet", userState.Language)}\n\n" +
                          $"{_localizationService.GetText("balance", userState.Language)}: €{balance:F2}";

                var keyboard = new InlineKeyboardMarkup(new[]
                {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(_localizationService.GetText("deposit", userState.Language), "wallet_deposit"),
                    InlineKeyboardButton.WithCallbackData(_localizationService.GetText("withdraw", userState.Language), "wallet_withdraw")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(_localizationService.GetText("transaction_history", userState.Language), "wallet_history")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(_localizationService.GetText("back", userState.Language), "back_main")
                }
            });

                await _botClient.SendMessage(
                    chatId,
                    text,
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);
            }
        }

        private async Task ShowSettingsAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🇺🇦 Українська", "lang_ua"),
                InlineKeyboardButton.WithCallbackData("🇺🇸 English", "lang_en"),
                InlineKeyboardButton.WithCallbackData("Русский", "lang_ru")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("back", language), "back_main")
            }
        });

            await _botClient.SendMessage(
                chatId,
                $"⚙️ {_localizationService.GetText("settings", language)}",
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }

        private async Task ShowHelpAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            var helpText = language switch
            {
                "ua" => "🆘 Допомога\n\n" +
                       "CS2 Duels - платформа для проведення турнірів та дуелів в Counter-Strike 2.\n\n" +
                       "Основні функції:\n" +
                       "⚔️ Створення та участь у дуелях 1v1, 2v2, 5v5\n" +
                       "🏆 Організація турнірів\n" +
                       "💰 Призові фонди\n" +
                       "📊 Система рейтингу\n\n" +
                       "Для початку роботи потрібна авторизація через Steam.",

                "en" => "🆘 Help\n\n" +
                       "CS2 Duels - platform for Counter-Strike 2 tournaments and duels.\n\n" +
                       "Main features:\n" +
                       "⚔️ Create and join 1v1, 2v2, 5v5 duels\n" +
                       "🏆 Tournament organization\n" +
                       "💰 Prize pools\n" +
                       "📊 Rating system\n\n" +
                       "Steam authorization required to start.",

                _ => "🆘 Помощь\n\n" +
                    "CS2 Duels - платформа для проведения турниров и дуэлей в Counter-Strike 2.\n\n" +
                    "Основные функции:\n" +
                    "⚔️ Создание и участие в дуэлях 1v1, 2v2, 5v5\n" +
                    "🏆 Организация турниров\n" +
                    "💰 Призовые фонды\n" +
                    "📊 Система рейтинга\n\n" +
                    "Для начала работы нужна авторизация через Steam."
            };

            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("back", language), "back_main")
            }
        });

            await _botClient.SendMessage(
                chatId,
                helpText,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }

        private async Task ShowTransactionHistoryAsync(long chatId, UserState userState, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(
                chatId,
                $"📜 {_localizationService.GetText("transaction_history", userState.Language)}\n\n" +
                $"{_localizationService.GetText("loading", userState.Language)}",
                cancellationToken: cancellationToken);
        }

        private static bool RequiresAuth(string data)
        {
            return data.StartsWith("duel_") ||
                   data.StartsWith("tournament_") ||
                   data.StartsWith("wallet_") ||
                   data.StartsWith("menu_duels") ||
                   data.StartsWith("menu_tournaments") ||
                   data.StartsWith("menu_profile") ||
                   data.StartsWith("menu_wallet");
        }
    }
}
