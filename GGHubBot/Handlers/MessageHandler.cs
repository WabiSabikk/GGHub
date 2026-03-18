using GGHubBot.Models;
using GGHubBot.Features.Duels;
using GGHubBot.Services;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using GGHubBot.Features.Main;
using Microsoft.Extensions.Configuration;

namespace GGHubBot.Handlers
{
    public class MessageHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserStateService _userStateService;
        private readonly LocalizationService _localizationService;
        private readonly ApiService _apiService;
        private readonly DuelService _duelService;
        private readonly DuelUIService _duelUIService;
        private readonly TournamentHandler _tournamentHandler;
        private readonly IConfiguration _configuration;
        private readonly MenuUIService _menuUIService;

        public MessageHandler(
            ITelegramBotClient botClient,
            UserStateService userStateService,
            LocalizationService localizationService,
            ApiService apiService,
            DuelService duelService,
            DuelUIService duelUIService,
            TournamentHandler tournamentHandler,
            IConfiguration configuration,
            MenuUIService menuUIService)
        {
            _botClient = botClient;
            _userStateService = userStateService;
            _localizationService = localizationService;
            _apiService = apiService;
            _duelService = duelService;
            _duelUIService = duelUIService;
            _tournamentHandler = tournamentHandler;
            _configuration = configuration;
            _menuUIService = menuUIService;
        }

        public async Task HandleAsync(Message message, CancellationToken cancellationToken)
        {
            if (message.From == null || message.Text == null)
                return;

            var userState = await _userStateService.GetOrCreateUserStateAsync(message.From.Id);
            var lang = userState.Language;

            Log.Information("Message from {UserId}: {Message}, State: {State}",
                message.From.Id, message.Text, userState.State);

            switch (message.Text)
            {
                case "/start":
                    await HandleStartAsync(message, userState, cancellationToken);
                    break;

                default:
                    await HandleStateBasedMessageAsync(message, userState, cancellationToken);
                    break;
            }
        }

        private async Task HandleStartAsync(Message message, UserState userState, CancellationToken cancellationToken)
        {
            if (userState.State == BotState.Start)
            {
                await ShowLanguageSelectionAsync(message.Chat.Id, cancellationToken);
                await _userStateService.UpdateUserStateAsync(message.From!.Id, BotState.LanguageSelection);
            }
            else
            {
                await ShowMainMenuAsync(message.Chat.Id, userState.Language, cancellationToken);
                await _userStateService.UpdateUserStateAsync(message.From!.Id, BotState.MainMenu);
            }
        }

        private async Task HandleStateBasedMessageAsync(Message message, UserState userState, CancellationToken cancellationToken)
        {
            var lang = userState.Language;

            switch (userState.State)
            {
                case BotState.CreateDuelEntryFee:
                    await HandleDuelEntryFeeAsync(message, userState, cancellationToken);
                    break;

                case BotState.CreateTournamentStep1:
                    await HandleTournamentTitleAsync(message, userState, cancellationToken);
                    break;

                case BotState.CreateTournamentStep2:
                    await HandleTournamentDescriptionAsync(message, userState, cancellationToken);
                    break;

                case BotState.WalletDeposit:
                case BotState.WalletWithdraw:
                    await HandleWalletAmountAsync(message, userState, cancellationToken);
                    break;

                case BotState.JoinDuel:
                    await HandleJoinByCodeAsync(message, userState, cancellationToken);
                    break;

                default:
                    if (!userState.IsAuthenticated || string.IsNullOrEmpty(userState.SteamId))
                    {
                        await ShowAuthRequiredAsync(message.Chat.Id, lang, cancellationToken);
                    }
                    else
                    {
                        await _botClient.SendMessage(
                            message.Chat.Id,
                            _localizationService.GetText("error", lang),
                            cancellationToken: cancellationToken);
                    }
                    break;
            }
        }

        private async Task HandleDuelEntryFeeAsync(Message message, UserState userState, CancellationToken cancellationToken)
        {
#if DEBUG
            var minFee = 0m;
#else
            var minFee = 5m;
#endif
            if (decimal.TryParse(message.Text, out var entryFee) && entryFee >= minFee)
            {
                var duelState = await _userStateService.GetStateDataAsync<CreateDuelState>(userState.TelegramId);
                if (duelState != null && duelState.CurrentStep == 2)
                {
                    duelState.EntryFee = entryFee;
                    duelState.CurrentStep = 3;
                    await _userStateService.UpdateUserStateAsync(userState.TelegramId, BotState.CreateDuelRounds, duelState);
                    await _duelUIService.ShowRoundFormatSelectionAsync(message.Chat.Id, userState.Language, cancellationToken);
                }
            }
            else
            {
                await _botClient.SendMessage(
                    message.Chat.Id,
                    _localizationService.GetText("invalid_amount", userState.Language),
                    cancellationToken: cancellationToken);
            }
        }

        private async Task HandleJoinByCodeAsync(Message message, UserState userState, CancellationToken cancellationToken)
        {
            if (!userState.UserId.HasValue || string.IsNullOrWhiteSpace(message.Text))
                return;

            var result = await _duelService.JoinDuelByCodeAsync(message.Text.Trim(), userState.UserId.Value);

            if (result?.Success == true)
            {
                await _botClient.SendMessage(
                    message.Chat.Id,
                    _localizationService.GetText("duel_joined", userState.Language),
                    cancellationToken: cancellationToken);
            }
            else
            {
                var msg = !string.IsNullOrEmpty(result?.Message) ? result!.Message : string.Join(", ", result?.Errors ?? new List<string>());
                var errorText = $"{_localizationService.GetText("error", userState.Language)}: {msg}";
                await _botClient.SendMessage(message.Chat.Id, errorText, cancellationToken: cancellationToken);
            }

            await _userStateService.UpdateUserStateAsync(userState.TelegramId, BotState.MainMenu);
            await ShowMainMenuAsync(message.Chat.Id, userState.Language, cancellationToken);
        }

        private async Task HandleTournamentTitleAsync(Message message, UserState userState, CancellationToken cancellationToken)
        {
            var tournamentState = await _userStateService.GetStateDataAsync<CreateTournamentState>(userState.TelegramId) ?? new();
            tournamentState.Title = message.Text;
            tournamentState.CurrentStep = 2;

            await _userStateService.UpdateUserStateAsync(userState.TelegramId, BotState.CreateTournamentStep2, tournamentState);
            await _tournamentHandler.ShowDescriptionStepAsync(message.Chat.Id, userState.Language, cancellationToken);
        }

        private async Task HandleTournamentDescriptionAsync(Message message, UserState userState, CancellationToken cancellationToken)
        {
            var tournamentState = await _userStateService.GetStateDataAsync<CreateTournamentState>(userState.TelegramId);
            if (tournamentState != null)
            {
                tournamentState.Description = message.Text;
                tournamentState.CurrentStep = 3;

                await _userStateService.UpdateUserStateAsync(userState.TelegramId, BotState.CreateTournamentStep3, tournamentState);
                await _tournamentHandler.ShowPlayersPerTeamStepAsync(message.Chat.Id, userState.Language, cancellationToken);
            }
        }

        private async Task HandleWalletAmountAsync(Message message, UserState userState, CancellationToken cancellationToken)
        {
            if (decimal.TryParse(message.Text, out var amount) && amount > 0)
            {
                var walletState = await _userStateService.GetStateDataAsync<WalletOperationState>(userState.TelegramId);
                if (walletState != null)
                {
                    walletState.Amount = amount;
                    await _userStateService.UpdateUserStateAsync(userState.TelegramId, userState.State, walletState);

                    if (userState.State == BotState.WalletDeposit)
                    {
                        await ShowDepositConfirmationAsync(message.Chat.Id, amount, userState.Language, cancellationToken);
                    }
                    else
                    {
                        await ShowWithdrawConfirmationAsync(message.Chat.Id, amount, userState.Language, cancellationToken);
                    }
                }
            }
            else
            {
                await _botClient.SendMessage(
                    message.Chat.Id,
                    _localizationService.GetText("invalid_amount", userState.Language),
                    cancellationToken: cancellationToken);
            }
        }

        private async Task ShowLanguageSelectionAsync(long chatId, CancellationToken cancellationToken)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🇺🇦 Українська", "lang_ua"),
                InlineKeyboardButton.WithCallbackData("🇺🇸 English", "lang_en")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Русский", "lang_ru")
            }
        });

            await _botClient.SendMessage(
                chatId,
                _localizationService.GetText("welcome"),
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }

        private Task ShowMainMenuAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            return _menuUIService.ShowMainMenuAsync(chatId, language, cancellationToken);
        }

        private async Task ShowAuthRequiredAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            var callbackUrl = _configuration["Steam:RedirectUrl"] ?? string.Empty;
            var authUrl = callbackUrl.Replace("callback", "auth") + $"?chatId={chatId}";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithWebApp(
                    _localizationService.GetText("login_steam", language),
                    new WebAppInfo { Url = authUrl })
            }
        });

            await _botClient.SendMessage(
                chatId,
                _localizationService.GetText("auth_required", language),
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }

        private async Task ShowDepositConfirmationAsync(long chatId, decimal amount, string language, CancellationToken cancellationToken)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("confirm", language), $"deposit_confirm_{amount}"),
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("cancel", language), "deposit_cancel")
            }
        });

            await _botClient.SendMessage(
                chatId,
                $"💰 {_localizationService.GetText("deposit", language)}: €{amount:F2}",
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }

        private async Task ShowWithdrawConfirmationAsync(long chatId, decimal amount, string language, CancellationToken cancellationToken)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("confirm", language), $"withdraw_confirm_{amount}"),
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("cancel", language), "withdraw_cancel")
            }
        });

            await _botClient.SendMessage(
                chatId,
                $"💸 {_localizationService.GetText("withdraw", language)}: €{amount:F2}",
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }
    }
}
