using GGHubBot.Models;
using GGHubBot.Services;
using GGHubShared.Models;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GGHubBot.Features.Duels
{
    public partial class DuelUIService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly LocalizationService _localizationService;
        private readonly AdminService _adminService;

        private readonly string[] _availableMaps =
        {
            "de_dust2", "de_mirage", "de_inferno", "de_overpass",
            "de_train", "de_nuke", "de_vertigo", "de_ancient", "de_anubis"
        };

        private string GetStepText(string language, int step, int total)
        {
            return $"{_localizationService.GetText("step", language)} {step} {_localizationService.GetText("of", language)} {total}";
        }

        public DuelUIService(
            ITelegramBotClient botClient,
            LocalizationService localizationService,
            AdminService adminService)
        {
            _botClient = botClient;
            _localizationService = localizationService;
            _adminService = adminService;
        }

        public async Task ShowFormatSelectionAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("1v1", "duel_format_1v1"),
                    InlineKeyboardButton.WithCallbackData("2v2", "duel_format_2v2"),
                    InlineKeyboardButton.WithCallbackData("5v5", "duel_format_5v5")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(_localizationService.GetText("back", language), "back_main")
                }
            });

            var stepText = GetStepText(language, 1, 7);
            var text = $"{stepText}\n\n{_localizationService.GetText("duel_format", language)}";

            await _botClient.SendMessage(chatId, text, replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        public async Task ShowEntryFeeStepAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            var stepText = GetStepText(language, 2, 7);
            var text = $"{stepText}\n\n{_localizationService.GetText("entry_fee", language)}";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(_localizationService.GetText("previous", language), "duel_step_back")
                }
            });

            await _botClient.SendMessage(chatId, text, replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        public async Task ShowRoundFormatSelectionAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Best of 1", "duel_rounds_bo1"),
                    InlineKeyboardButton.WithCallbackData("Best of 3", "duel_rounds_bo3"),
                    InlineKeyboardButton.WithCallbackData("Best of 5", "duel_rounds_bo5")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(_localizationService.GetText("previous", language), "duel_step_back")
                }
            });

            var stepText = GetStepText(language, 3, 7);
            var text = $"{stepText}\n\n{_localizationService.GetText("round_format", language)}";

            await _botClient.SendMessage(chatId, text, replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        public async Task ShowPrimeSelectionAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            var stepText = GetStepText(language, 4, 7);
            var text = $"{stepText}\n\n{_localizationService.GetText("prime_only", language)}";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Yes", "duel_prime_yes"),
                    InlineKeyboardButton.WithCallbackData("No", "duel_prime_no")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(_localizationService.GetText("previous", language), "duel_step_back")
                }
            });

            await _botClient.SendMessage(chatId, text, replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        public async Task ShowWarmupSelectionAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            var stepText = GetStepText(language, 5, 7);
            var text = $"{stepText}\n\n{_localizationService.GetText("warmup_time", language)}";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("3 min", "duel_warmup_3"),
                    InlineKeyboardButton.WithCallbackData("5 min", "duel_warmup_5")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(_localizationService.GetText("previous", language), "duel_step_back")
                }
            });

            await _botClient.SendMessage(chatId, text, replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        public async Task ShowMaxRoundsSelectionAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            var stepText = GetStepText(language, 6, 7);
            var text = $"{stepText}\n\n{_localizationService.GetText("max_rounds", language)}";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("16", "duel_maxrounds_16"),
                    InlineKeyboardButton.WithCallbackData("24", "duel_maxrounds_24"),
                    InlineKeyboardButton.WithCallbackData("30", "duel_maxrounds_30")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(_localizationService.GetText("previous", language), "duel_step_back")
                }
            });

            await _botClient.SendMessage(chatId, text, replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        public async Task ShowMapSelectionAsync(long chatId, CreateDuelState duelState, string language, CancellationToken cancellationToken)
        {
            var buttons = new List<InlineKeyboardButton[]>();

            for (int i = 0; i < _availableMaps.Length; i += 2)
            {
                var row = new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData(_availableMaps[i], $"duel_map_{_availableMaps[i]}")
                };

                if (i + 1 < _availableMaps.Length)
                {
                    row.Add(InlineKeyboardButton.WithCallbackData(_availableMaps[i + 1], $"duel_map_{_availableMaps[i + 1]}"));
                }

                buttons.Add(row.ToArray());
            }

            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("next", language), "duel_maps_done"),
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("previous", language), "duel_step_back")
            });

            var keyboard = new InlineKeyboardMarkup(buttons);

            var stepText = GetStepText(language, 7, 7);
            var text = $"{stepText}\n\n{_localizationService.GetText("select_maps", language)}";
            if (duelState.Maps.Any())
            {
                text += "\n\n" + string.Join(", ", duelState.Maps);
            }

            await _botClient.SendMessage(chatId, text, replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        public async Task UpdateMapSelectionAsync(long chatId, int messageId, CreateDuelState duelState, string language, CancellationToken cancellationToken)
        {
            var buttons = new List<InlineKeyboardButton[]>();
            for (int i = 0; i < _availableMaps.Length; i += 2)
            {
                var row = new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData(_availableMaps[i], $"duel_map_{_availableMaps[i]}")
                };

                if (i + 1 < _availableMaps.Length)
                {
                    row.Add(InlineKeyboardButton.WithCallbackData(_availableMaps[i + 1], $"duel_map_{_availableMaps[i + 1]}"));
                }

                buttons.Add(row.ToArray());
            }

            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("next", language), "duel_maps_done"),
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("previous", language), "duel_step_back")
            });

            var keyboard = new InlineKeyboardMarkup(buttons);

            var stepText = GetStepText(language, 7, 7);
            var text = $"{stepText}\n\n{_localizationService.GetText("select_maps", language)}";
            if (duelState.Maps.Any())
            {
                text += "\n\n" + string.Join(", ", duelState.Maps);
            }

            await _botClient.EditMessageText(chatId, messageId, text, replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        public async Task ShowAvailableDuelsAsync(long chatId, List<DuelDto> duels, string language, CancellationToken cancellationToken)
        {
            var buttons = new List<InlineKeyboardButton[]>();

            foreach (var duel in duels.Take(10))
            {
                var prime = duel.PrimeOnly ? " (Prime)" : string.Empty;
                var buttonText = $"{duel.Format} - €{duel.EntryFee:F2}{prime}";
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(buttonText, $"duel_view_{duel.Id}")
                });
            }

            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("back", language), "back_main")
            });

            var keyboard = new InlineKeyboardMarkup(buttons);

            await _botClient.SendMessage(chatId, $"🔍 {_localizationService.GetText("find_duel", language)}", replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        public async Task ShowMyDuelsAsync(long chatId, List<DuelDto> duels, Guid? userId, string language, CancellationToken cancellationToken)
        {
            var text = $"📋 {_localizationService.GetText("my_duels", language)}\n\n";

            foreach (var duel in duels.Take(5))
            {
                var prime = duel.PrimeOnly ? " (Prime)" : string.Empty;
                text += $"🎮 {duel.Format} - €{duel.EntryFee:F2}{prime}\n";
                text += $"📊 Status: {duel.Status}\n";
                text += $"👥 Players: {duel.CurrentParticipants}/{duel.MaxParticipants}\n";
                if (!string.IsNullOrEmpty(duel.InviteLink))
                {
                    text += $"🔗 Invite Code: `{duel.InviteLink}`\n";
                }
                var participant = duel.Participants.FirstOrDefault(p => p.User.Id == userId);
                var hasPaid = participant?.HasPaid == true;
                var allReady = duel.Participants.All(p => p.IsReady);
                if (duel.Status == GGHubShared.Enums.DuelStatus.PaymentPending && hasPaid)
                {
                    text += _localizationService.GetText("waiting_opponent", language) + "\n";
                }
                if (duel.GameServer != null && allReady)
                {
                    //var steamUrl = $"steam://connect/{duel.GameServer.ServerIp}:{duel.GameServer.ServerPort}/{duel.GameServer.Password}";
                    text += $"{_localizationService.GetText("server", language)}: `{duel.GameServer.ServerIp}:{duel.GameServer.ServerPort}`\n";
                    text += $"Steam URL: `{duel.GameServer.SteamUrl}`\n";
                    text += $"{_localizationService.GetText("password", language)}: `{duel.GameServer.Password}`\n";
                }
                text += "\n";
            }

            var buttons = new List<InlineKeyboardButton[]>();
            var isAdmin = _adminService.IsAdmin(chatId);
            foreach (var duel in duels.Take(5))
            {
                var participant = duel.Participants.FirstOrDefault(p => p.User.Id == userId);
                var hasPaid = participant?.HasPaid == true;
                var allReady = duel.Participants.All(p => p.IsReady);
                if (duel.Status == GGHubShared.Enums.DuelStatus.PaymentPending)
                {
                    if (!hasPaid)
                    {
                        buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(_localizationService.GetText("pay_balance", language), $"payment_balance_{duel.Id}") });
                        buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(_localizationService.GetText("pay_cryptomus", language), $"payment_crypto_{duel.Id}") });
                    }
                }
                else if (duel.GameServer != null && !allReady && hasPaid)
                {
                    buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(_localizationService.GetText("ready", language), $"duel_ready_{duel.Id}") });
                }
                else if (duel.GameServer != null && allReady && isAdmin)
                {
                    buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(_localizationService.GetText("get_server", language), $"duel_get_server_{duel.Id}") });
                }

                // Кнопка "Покинути матч" для активних дуелей
                if (duel.Status == GGHubShared.Enums.DuelStatus.InProgress ||
                    duel.Status == GGHubShared.Enums.DuelStatus.Starting ||
                    duel.Status == GGHubShared.Enums.DuelStatus.WaitingForLaunch)
                {
                    buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(_localizationService.GetText("forfeit_match", language), $"duel_forfeit_{duel.Id}") });
                }
            }
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(_localizationService.GetText("back", language), "back_main") });
            var keyboard = new InlineKeyboardMarkup(buttons);

            await _botClient.SendMessage(chatId, text, replyMarkup: keyboard, parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
        }

        public async Task ShowDuelCreatedAsync(long chatId, int messageId, DuelDto duel, string inviteCode, string language, CancellationToken cancellationToken)
        {
            var text = $"*{_localizationService.GetText("duel_created", language)}*\n\n" +
                       $"ID: {duel.Id}\n";

            if (!string.IsNullOrEmpty(inviteCode))
            {
                text += $"Invite Code: `{inviteCode}`\n";
            }

            var keyboard = new InlineKeyboardMarkup(
                InlineKeyboardButton.WithCallbackData(_localizationService.GetText("main_menu", language), "back_main"));

            text += "\n" + $"_{_localizationService.GetText("duel_invite_rules", language)}_";

            await _botClient.EditMessageText(chatId, messageId, text, parseMode: ParseMode.Markdown, replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        public async Task ShowDuelPaymentInfoAsync(long chatId, DuelDto duel, Guid? userId, string language, CancellationToken cancellationToken)
        {
            var prime = duel.PrimeOnly ? " (Prime)" : string.Empty;
            var text = $"{duel.Format} - €{duel.EntryFee:F2}{prime}\n" +
                       $"📊 Status: {duel.Status}\n" +
                       $"👥 Players: {duel.CurrentParticipants}/{duel.MaxParticipants}";

            var participant = duel.Participants.FirstOrDefault(p => p.User.Id == userId);
            var hasPaid = participant?.HasPaid == true;
            var isAdmin = _adminService.IsAdmin(chatId);
            InlineKeyboardMarkup? keyboard;

            if (hasPaid && duel.GameServer != null && isAdmin)
            {
                keyboard = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(_localizationService.GetText("get_server", language), $"duel_get_server_{duel.Id}"));
            }
            else
            {
                keyboard = new InlineKeyboardMarkup(new[]
                {
                    new [] { InlineKeyboardButton.WithCallbackData(_localizationService.GetText("pay_balance", language), $"payment_balance_{duel.Id}") },
                    new [] { InlineKeyboardButton.WithCallbackData(_localizationService.GetText("pay_cryptomus", language), $"payment_crypto_{duel.Id}") }
                });
            }

            await _botClient.SendMessage(chatId, text, replyMarkup: keyboard, cancellationToken: cancellationToken);
        }
        public async Task ShowDuelConfirmationAsync(long chatId, int messageId, CreateDuelState duelState, string language, CancellationToken cancellationToken)
        {
            var format = duelState.Format switch
            {
                "1v1" => "1v1",
                "2v2" => "2v2",
                "5v5" => "5v5",
                _ => "Unknown"
            };

            var maps = string.Join(", ", duelState.Maps);
            var commission = (duelState.EntryFee ?? 0) * DuelService.GetPlayersCount(duelState.Format!) * 0.10m;
            var prizeFund = (duelState.EntryFee ?? 0) * DuelService.GetPlayersCount(duelState.Format!) - commission;

            var text = $"{_localizationService.GetText("confirm_duel", language)}\n\n" +
                       $"🎮 Format: {format}\n" +
                       $"💰 Entry Fee: €{duelState.EntryFee:F2}\n" +
                       $"🗺️ Maps: {maps}\n" +
                       $"🎯 Rounds: {duelState.RoundFormat}\n" +
                       $"🔢 Max Rounds: {duelState.MaxRounds}\n" +
                       $"⏱ Warmup: {duelState.WarmupMinutes} min\n" +
                       (duelState.PrimeOnly ? "(Prime)\n" : string.Empty) +
                       $"🏆 Prize Fund: €{prizeFund:F2}\n" +
                       $"💸 Commission (10%): €{commission:F2}";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(_localizationService.GetText("confirm", language), "duel_confirm_create"),
                    InlineKeyboardButton.WithCallbackData(_localizationService.GetText("cancel", language), "back_main")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(_localizationService.GetText("previous", language), "duel_step_back")
                }
            });

            await _botClient.EditMessageText(chatId, messageId, text, replyMarkup: keyboard, cancellationToken: cancellationToken);
        }


        public async Task PromptJoinByCodeAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(chatId, _localizationService.GetText("enter_invite_code", language), cancellationToken: cancellationToken);
        }

        public async Task ShowGetServerAsync(long chatId, DuelDto duel, string language, CancellationToken cancellationToken)
        {
            if (duel.GameServer == null)
                return;

            var server = duel.GameServer;
            var steamUrl = server.SteamUrl;

            var text = $"🖥️ <b>{_localizationService.GetText("server", language)}:</b> <code>{server.ServerIp}:{server.ServerPort}</code>\n" +
                       $"🔑 <b>{_localizationService.GetText("password", language)}:</b> <code>{server.Password}</code>\n\n" +
                       $"🎮 <b>{_localizationService.GetText("join_steam", language)}:</b>\n" +
                       $"<pre>{steamUrl}</pre>";

            await _botClient.SendMessage(chatId, text,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }
        public async Task ShowReadyPromptAsync(long chatId, Guid duelId, string language, CancellationToken cancellationToken)
        {
            var keyboard = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(
                _localizationService.GetText("ready", language), $"duel_ready_{duelId}"));
            await _botClient.SendMessage(chatId,
                _localizationService.GetText("press_ready", language),
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }

        public async Task ShowErrorAsync(long chatId, string message, string language, CancellationToken cancellationToken)
        {
            var errorText = $"{_localizationService.GetText("error", language)}: {message}";
            await _botClient.SendMessage(chatId, errorText, cancellationToken: cancellationToken);
        }

        public async Task ShowSuccessAsync(long chatId, string messageKey, string language, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(chatId, _localizationService.GetText(messageKey, language), cancellationToken: cancellationToken);
        }
        public async Task ShowServerStartingAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(chatId, _localizationService.GetText("server_start", language), cancellationToken: cancellationToken);
        }

        public async Task ShowNoDataAsync(long chatId, string language, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(chatId, _localizationService.GetText("no_data", language), cancellationToken: cancellationToken);
        }
    }
}
