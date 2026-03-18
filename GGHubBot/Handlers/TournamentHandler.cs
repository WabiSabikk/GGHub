using GGHubBot.Models;
using GGHubBot.Services;
using GGHubShared.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GGHubBot.Handlers
{
    public class TournamentHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserStateService _userStateService;
        private readonly LocalizationService _localizationService;
        private readonly ApiService _apiService;
        private readonly MediaService _mediaService;

        private readonly string[] _availableMaps =
        {
        "de_dust2", "de_mirage", "de_inferno", "de_cache", "de_overpass",
        "de_train", "de_nuke", "de_vertigo", "de_ancient", "de_anubis"
    };

        public TournamentHandler(
            ITelegramBotClient botClient,
            UserStateService userStateService,
            LocalizationService localizationService,
            ApiService apiService,
            MediaService mediaService)
        {
            _botClient = botClient;
            _userStateService = userStateService;
            _localizationService = localizationService;
            _apiService = apiService;
            _mediaService = mediaService;
        }

        public async Task HandleCallbackAsync(CallbackQuery callbackQuery, UserState userState, string data, CancellationToken cancellationToken)
        {
            var chatId = callbackQuery.Message!.Chat.Id;
            var lang = userState.Language;

            switch (data)
            {
                case "tournament_create":
                    await StartCreateTournamentAsync(chatId, userState, cancellationToken);
                    break;

                case "tournament_available":
                    await ShowAvailableTournamentsAsync(chatId, userState, cancellationToken);
                    break;

                case "tournament_my":
                    await ShowMyTournamentsAsync(chatId, userState, cancellationToken);
                    break;

                case var d when d.StartsWith("tournament_players_"):
                    await HandlePlayersPerTeamSelectionAsync(chatId, userState, d, cancellationToken);
                    break;

                case var d when d.StartsWith("tournament_teams_"):
                    await HandleMaxTeamsSelectionAsync(chatId, userState, d, cancellationToken);
                    break;

                case var d when d.StartsWith("tournament_map_"):
                    await HandleTournamentMapSelectionAsync(chatId, userState, d, callbackQuery.Id, cancellationToken);
                    break;

                case "tournament_maps_done":
                    await ShowStartTimeStepAsync(chatId, userState, cancellationToken);
                    break;

                case "tournament_skip_description":
                    await ShowPlayersPerTeamStepAsync(chatId, userState.Language, cancellationToken);
                    break;

                case "tournament_skip_start_time":
                    await ShowRulesStepAsync(chatId, userState, cancellationToken);
                    break;

                case "tournament_skip_rules":
                    await ShowTournamentConfirmationAsync(chatId, userState, cancellationToken);
                    break;

                case "tournament_confirm_create":
                    await ConfirmTournamentCreationAsync(chatId, userState, cancellationToken);
                    break;

                case "tournament_step_back":
                    await HandleTournamentStepBackAsync(chatId, userState, cancellationToken);
                    break;

                case var d when d.StartsWith("tournament_view_"):
                    await ViewTournamentDetailsAsync(chatId, userState, d, cancellationToken);
                    break;

                case var d when d.StartsWith("tournament_join_"):
                    await JoinTournamentAsync(chatId, userState, d, cancellationToken);
                    break;
            }
        }

        private async Task StartCreateTournamentAsync(long chatId, UserState userState, CancellationToken cancellationToken)
        {
            var tournamentState = new CreateTournamentState { CurrentStep = 1 };
            await _userStateService.UpdateUserStateAsync(userState.TelegramId, BotState.CreateTournamentStep1, tournamentState);

            var stepText = $"{_localizationService.GetText("step", userState.Language)} 1 {_localizationService.GetText("of", userState.Language)} 6";
            var text = $"{stepText}\n\n{_localizationService.GetText("tournament_title", userState.Language)}";

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

        public async Task ShowDescriptionStepAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            var stepText = $"{_localizationService.GetText("step", language)} 2 {_localizationService.GetText("of", language)} 6";
            var text = $"{stepText}\n\n{_localizationService.GetText("tournament_description", language)}";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("⏭️ Skip", "tournament_skip_description"),
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("previous", language), "tournament_step_back")
            }
        });

            await _botClient.SendMessage(
                chatId,
                text,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }

        public async Task ShowPlayersPerTeamStepAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            var stepText = $"{_localizationService.GetText("step", language)} 3 {_localizationService.GetText("of", language)} 6";
            var text = $"{stepText}\n\n{_localizationService.GetText("players_per_team", language)}";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("1v1", "tournament_players_1"),
                InlineKeyboardButton.WithCallbackData("2v2", "tournament_players_2"),
                InlineKeyboardButton.WithCallbackData("5v5", "tournament_players_5")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("previous", language), "tournament_step_back")
            }
        });

            await _botClient.SendMessage(
                chatId,
                text,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }

        private async Task HandlePlayersPerTeamSelectionAsync(long chatId, UserState userState, string data, CancellationToken cancellationToken)
        {
            var playersCount = int.Parse(data.Split('_')[2]);
            var tournamentState = await _userStateService.GetStateDataAsync<CreateTournamentState>(userState.TelegramId);

            if (tournamentState != null)
            {
                tournamentState.PlayersPerTeam = playersCount;
                tournamentState.CurrentStep = 4;
                await _userStateService.UpdateUserStateAsync(userState.TelegramId, BotState.CreateTournamentStep4, tournamentState);

                await ShowMaxTeamsStepAsync(chatId, userState.Language, cancellationToken);
            }
        }

        private async Task ShowMaxTeamsStepAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            var stepText = $"{_localizationService.GetText("step", language)} 4 {_localizationService.GetText("of", language)} 6";
            var text = $"{stepText}\n\n{_localizationService.GetText("max_teams", language)}";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("4", "tournament_teams_4"),
                InlineKeyboardButton.WithCallbackData("8", "tournament_teams_8"),
                InlineKeyboardButton.WithCallbackData("16", "tournament_teams_16")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("32", "tournament_teams_32")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("previous", language), "tournament_step_back")
            }
        });

            await _botClient.SendMessage(
                chatId,
                text,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }

        private async Task HandleMaxTeamsSelectionAsync(long chatId, UserState userState, string data, CancellationToken cancellationToken)
        {
            var maxTeams = int.Parse(data.Split('_')[2]);
            var tournamentState = await _userStateService.GetStateDataAsync<CreateTournamentState>(userState.TelegramId);

            if (tournamentState != null)
            {
                tournamentState.MaxTeams = maxTeams;
                tournamentState.CurrentStep = 5;
                await _userStateService.UpdateUserStateAsync(userState.TelegramId, BotState.CreateTournamentStep5, tournamentState);

                await ShowEntryFeeStepAsync(chatId, userState, cancellationToken);
            }
        }

        private async Task ShowEntryFeeStepAsync(long chatId, UserState userState, CancellationToken cancellationToken)
        {
            var stepText = $"{_localizationService.GetText("step", userState.Language)} 5 {_localizationService.GetText("of", userState.Language)} 6";
            var text = $"{stepText}\n\n{_localizationService.GetText("tournament_entry_fee", userState.Language)}";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("€10", "tournament_fee_10"),
                InlineKeyboardButton.WithCallbackData("€25", "tournament_fee_25"),
                InlineKeyboardButton.WithCallbackData("€50", "tournament_fee_50")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("€100", "tournament_fee_100"),
                InlineKeyboardButton.WithCallbackData("€250", "tournament_fee_250"),
                InlineKeyboardButton.WithCallbackData("€500", "tournament_fee_500")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("previous", userState.Language), "tournament_step_back")
            }
        });

            await _botClient.SendMessage(
                chatId,
                text,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }

        private async Task ShowMapSelectionStepAsync(long chatId, UserState userState, CancellationToken cancellationToken)
        {
            var buttons = new List<InlineKeyboardButton[]>();

            for (int i = 0; i < _availableMaps.Length; i += 2)
            {
                var row = new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(_availableMaps[i], $"tournament_map_{_availableMaps[i]}")
            };

                if (i + 1 < _availableMaps.Length)
                {
                    row.Add(InlineKeyboardButton.WithCallbackData(_availableMaps[i + 1], $"tournament_map_{_availableMaps[i + 1]}"));
                }

                buttons.Add(row.ToArray());
            }

            buttons.Add(new[]
            {
            InlineKeyboardButton.WithCallbackData(_localizationService.GetText("next", userState.Language), "tournament_maps_done"),
            InlineKeyboardButton.WithCallbackData(_localizationService.GetText("previous", userState.Language), "tournament_step_back")
        });

            var keyboard = new InlineKeyboardMarkup(buttons);

            var stepText = $"{_localizationService.GetText("step", userState.Language)} 6 {_localizationService.GetText("of", userState.Language)} 6";
            var text = $"{stepText}\n\n{_localizationService.GetText("select_maps", userState.Language)}";

            await _botClient.SendMessage(
                chatId,
                text,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }

        private async Task HandleTournamentMapSelectionAsync(long chatId, UserState userState, string data, string callbackQueryId, CancellationToken cancellationToken)
        {
            var mapName = data.Split('_')[2];
            var tournamentState = await _userStateService.GetStateDataAsync<CreateTournamentState>(userState.TelegramId);

            if (tournamentState != null)
            {
                if (tournamentState.Maps.Contains(mapName))
                {
                    tournamentState.Maps.Remove(mapName);
                }
                else
                {
                    tournamentState.Maps.Add(mapName);
                }

                await _userStateService.UpdateUserStateAsync(userState.TelegramId, userState.State, tournamentState);

                var selectedMaps = string.Join(", ", tournamentState.Maps);
                var text = $"✅ Selected maps: {selectedMaps}";

                await _botClient.AnswerCallbackQuery(
                    callbackQueryId,
                    text: text,
                    cancellationToken: cancellationToken);
            }
        }

        private async Task ShowStartTimeStepAsync(long chatId, UserState userState, CancellationToken cancellationToken)
        {
            var text = "📅 When should the tournament start?\n\nPlease send the start time (e.g., '2024-01-15 19:00') or skip this step.";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("⏭️ Skip", "tournament_skip_start_time"),
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("previous", userState.Language), "tournament_step_back")
            }
        });

            await _botClient.SendMessage(
                chatId,
                text,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }

        private async Task ShowRulesStepAsync(long chatId, UserState userState, CancellationToken cancellationToken)
        {
            var text = "📋 Tournament rules (optional)\n\nPlease send tournament rules or skip this step.";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("⏭️ Skip", "tournament_skip_rules"),
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("previous", userState.Language), "tournament_step_back")
            }
        });

            await _botClient.SendMessage(
                chatId,
                text,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }

        private async Task ShowTournamentConfirmationAsync(long chatId, UserState userState, CancellationToken cancellationToken)
        {
            var tournamentState = await _userStateService.GetStateDataAsync<CreateTournamentState>(userState.TelegramId);

            if (tournamentState != null)
            {
                var totalEntryFees = (tournamentState.EntryFee ?? 0) * (tournamentState.MaxTeams ?? 0);
                var commission = totalEntryFees * 0.10m;
                var prizeFund = totalEntryFees - commission;

                var maps = tournamentState.Maps.Any() ? string.Join(", ", tournamentState.Maps) : "TBD";
                var startTime = tournamentState.StartTime?.ToString("yyyy-MM-dd HH:mm") ?? "TBD";

                var text = $"✅ {_localizationService.GetText("confirm", userState.Language)}\n\n" +
                          $"🏆 Title: {tournamentState.Title}\n" +
                          $"📝 Description: {tournamentState.Description ?? "None"}\n" +
                          $"👥 Players per team: {tournamentState.PlayersPerTeam}\n" +
                          $"🏢 Max teams: {tournamentState.MaxTeams}\n" +
                          $"💰 Entry fee: €{tournamentState.EntryFee:F2}\n" +
                          $"🗺️ Maps: {maps}\n" +
                          $"📅 Start time: {startTime}\n" +
                          $"📋 Rules: {tournamentState.Rules ?? "Standard"}\n\n" +
                          $"🏆 Prize fund: €{prizeFund:F2}\n" +
                          $"💸 Commission (10%): €{commission:F2}";

                var keyboard = new InlineKeyboardMarkup(new[]
                {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(_localizationService.GetText("confirm", userState.Language), "tournament_confirm_create"),
                    InlineKeyboardButton.WithCallbackData(_localizationService.GetText("cancel", userState.Language), "back_main")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(_localizationService.GetText("previous", userState.Language), "tournament_step_back")
                }
            });

                await _botClient.SendMessage(
                    chatId,
                    text,
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);
            }
        }

        private async Task ConfirmTournamentCreationAsync(long chatId, UserState userState, CancellationToken cancellationToken)
        {
            var tournamentState = await _userStateService.GetStateDataAsync<CreateTournamentState>(userState.TelegramId);

            if (tournamentState != null && userState.UserId.HasValue)
            {
                var request = new CreateTournamentRequest
                {
                    Title = tournamentState.Title!,
                    Description = tournamentState.Description,
                    PlayersPerTeam = tournamentState.PlayersPerTeam!.Value,
                    MaxTeams = tournamentState.MaxTeams!.Value,
                    EntryFee = tournamentState.EntryFee!.Value,
                    Maps = tournamentState.Maps,
                    StartTime = tournamentState.StartTime,
                    Rules = tournamentState.Rules
                };

                var result = await _apiService.CreateTournamentAsync(request, userState.UserId.Value);

                if (result?.Success == true)
                {
                    var tournament = result.Data!;
                    var inviteText = $"🏆 Tournament created!\n\n" +
                                   $"📊 ID: {tournament.Id}\n" +
                                   $"🎯 Title: {tournament.Title}\n" +
                                   $"💰 Prize fund: €{tournament.PrizeFund:F2}\n\n" +
                                   $"Share this tournament: /tournament_{tournament.Id}";

                    await _botClient.SendMessage(
                        chatId,
                        inviteText,
                        cancellationToken: cancellationToken);

                    await _userStateService.UpdateUserStateAsync(userState.TelegramId, BotState.MainMenu);
                    await _userStateService.ClearStateDataAsync(userState.TelegramId);
                }
                else
                {
                    var msg = !string.IsNullOrEmpty(result?.Message) ? result!.Message : string.Join(", ", result?.Errors ?? new List<string>());
                    await _botClient.SendMessage(
                        chatId,
                        $"{_localizationService.GetText("error", userState.Language)}: {msg}",
                        cancellationToken: cancellationToken);
                }
            }
        }

        private async Task ShowAvailableTournamentsAsync(long chatId, UserState userState, CancellationToken cancellationToken)
        {
            var tournaments = await _apiService.GetAvailableTournamentsAsync();

            if (tournaments?.Success == true && tournaments.Data?.Any() == true)
            {
                var buttons = new List<InlineKeyboardButton[]>();

                foreach (var tournament in tournaments.Data.Take(10))
                {
                    var buttonText = $"{tournament.Title} - €{tournament.EntryFee:F2}";
                    buttons.Add(new[]
                    {
                    InlineKeyboardButton.WithCallbackData(buttonText, $"tournament_view_{tournament.Id}")
                });
                }

                buttons.Add(new[]
                {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("back", userState.Language), "back_main")
            });

                var keyboard = new InlineKeyboardMarkup(buttons);

                await _botClient.SendMessage(
                    chatId,
                    $"🏆 {_localizationService.GetText("available_tournaments", userState.Language)}",
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);
            }
            else
            {
                await _botClient.SendMessage(
                    chatId,
                    _localizationService.GetText("no_data", userState.Language),
                    cancellationToken: cancellationToken);
            }
        }

        private async Task ShowMyTournamentsAsync(long chatId, UserState userState, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(
                chatId,
                $"👥 {_localizationService.GetText("my_tournaments", userState.Language)}\n\n{_localizationService.GetText("loading", userState.Language)}",
                cancellationToken: cancellationToken);
        }

        private async Task ViewTournamentDetailsAsync(long chatId, UserState userState, string data, CancellationToken cancellationToken)
        {
            var tournamentId = data.Split('_')[2];

            await _botClient.SendMessage(
                chatId,
                $"🏆 Tournament Details\n\nTournament ID: {tournamentId}\n\n{_localizationService.GetText("loading", userState.Language)}",
                cancellationToken: cancellationToken);
        }

        private async Task JoinTournamentAsync(long chatId, UserState userState, string data, CancellationToken cancellationToken)
        {
            var tournamentId = data.Split('_')[2];

            await _botClient.SendMessage(
                chatId,
                $"🏆 Joining tournament: {tournamentId}",
                cancellationToken: cancellationToken);
        }

        private async Task HandleTournamentStepBackAsync(long chatId, UserState userState, CancellationToken cancellationToken)
        {
            var tournamentState = await _userStateService.GetStateDataAsync<CreateTournamentState>(userState.TelegramId);

            if (tournamentState != null)
            {
                switch (tournamentState.CurrentStep)
                {
                    case 2:
                        await StartCreateTournamentAsync(chatId, userState, cancellationToken);
                        break;

                    case 3:
                        await ShowDescriptionStepAsync(chatId, userState.Language, cancellationToken);
                        tournamentState.CurrentStep = 2;
                        await _userStateService.UpdateUserStateAsync(userState.TelegramId, BotState.CreateTournamentStep2, tournamentState);
                        break;

                    case 4:
                        await ShowPlayersPerTeamStepAsync(chatId, userState.Language, cancellationToken);
                        tournamentState.CurrentStep = 3;
                        await _userStateService.UpdateUserStateAsync(userState.TelegramId, BotState.CreateTournamentStep3, tournamentState);
                        break;

                    case 5:
                        await ShowMaxTeamsStepAsync(chatId, userState.Language, cancellationToken);
                        tournamentState.CurrentStep = 4;
                        await _userStateService.UpdateUserStateAsync(userState.TelegramId, BotState.CreateTournamentStep4, tournamentState);
                        break;

                    case 6:
                        await ShowEntryFeeStepAsync(chatId, userState, cancellationToken);
                        tournamentState.CurrentStep = 5;
                        await _userStateService.UpdateUserStateAsync(userState.TelegramId, BotState.CreateTournamentStep5, tournamentState);
                        break;
                }
            }
        }
    }
}
