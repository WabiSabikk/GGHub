using GGHubBot.Features.Common;
using GGHubBot.Models;
using GGHubBot.Services;
using GGHubBot.Features.Payments;
using GGHubShared.Enums;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GGHubBot.Features.Duels
{
    public class DuelCallbackHandler : ICallbackCommand
    {
        private readonly DuelService _duelService;
        private readonly DuelUIService _duelUIService;
        private readonly UserStateService _userStateService;
        private readonly PaymentUIService _paymentUIService;
        private readonly ITelegramBotClient _botClient;
        private readonly AdminService _adminService;

        public DuelCallbackHandler(
            DuelService duelService,
            DuelUIService duelUIService,
            UserStateService userStateService,
            PaymentUIService paymentUIService,
            ITelegramBotClient botClient,
            AdminService adminService)
        {
            _duelService = duelService;
            _duelUIService = duelUIService;
            _userStateService = userStateService;
            _paymentUIService = paymentUIService;
            _botClient = botClient;
            _adminService = adminService;
        }

        public bool CanHandle(string callbackData) => callbackData.StartsWith("duel_");

        public async Task HandleAsync(CallbackQuery callbackQuery, UserState userState, CancellationToken cancellationToken)
        {
            var chatId = callbackQuery.Message!.Chat.Id;
            var data = callbackQuery.Data!;

            try
            {
                switch (data)
                {
                    case "duel_create":
                        await StartCreateDuelAsync(chatId, userState, cancellationToken);
                        break;
                    case "duel_find":
                        await ShowAvailableDuelsAsync(chatId, userState, cancellationToken);
                        break;
                    case "duel_my":
                        await ShowMyDuelsAsync(chatId, userState, cancellationToken);
                        break;
                    case "duel_join_code":
                        await _duelUIService.PromptJoinByCodeAsync(chatId, userState.Language, cancellationToken);
                        await _userStateService.UpdateUserStateAsync(userState.TelegramId, BotState.JoinDuel);
                        break;
                    case var d when d.StartsWith("duel_format_"):
                        await HandleFormatSelectionAsync(chatId, userState, d, cancellationToken);
                        break;
                    case var d when d.StartsWith("duel_map_"):
                        await HandleMapSelectionAsync(chatId, userState, d, callbackQuery, cancellationToken);
                        break;
                    case var d when d.StartsWith("duel_rounds_"):
                        await HandleRoundFormatSelectionAsync(chatId, userState, d, cancellationToken);
                        break;
                    case "duel_prime_yes":
                    case "duel_prime_no":
                        await HandlePrimeSelectionAsync(chatId, userState, data, cancellationToken);
                        break;
                    case var d when d.StartsWith("duel_warmup_"):
                        await HandleWarmupSelectionAsync(chatId, userState, d, cancellationToken);
                        break;
                    case var d when d.StartsWith("duel_maxrounds_"):
                        await HandleMaxRoundsSelectionAsync(chatId, userState, d, cancellationToken);
                        break;
                    case "duel_confirm_create":
                        await ConfirmDuelCreationAsync(chatId, userState, callbackQuery, cancellationToken);
                        break;
                    case var d when d.StartsWith("duel_join_"):
                        await JoinDuelAsync(chatId, userState, d, cancellationToken);
                        break;
                    case var d when d.StartsWith("duel_view_"):
                        await ViewDuelDetailsAsync(chatId, userState, d, cancellationToken);
                        break;
                    case var d when d.StartsWith("duel_get_server_"):
                        await HandleGetServerAsync(chatId, userState, d, cancellationToken);
                        break;
                    case var d when d.StartsWith("duel_ready_"):
                        await HandleReadyAsync(chatId, userState, d, cancellationToken);
                        break;
                    case var d when d.StartsWith("duel_forfeit_confirm_"):
                        await HandleForfeitConfirmAsync(chatId, userState, d, cancellationToken);
                        break;
                    case var d when d.StartsWith("duel_forfeit_badping_"):
                        await HandleForfeitBadPingAsync(chatId, userState, d, cancellationToken);
                        break;
                    case var d when d.StartsWith("duel_forfeit_"):
                        await HandleForfeitInitiateAsync(chatId, userState, d, cancellationToken);
                        break;
                    case "duel_cancel_forfeit":
                        await ShowMyDuelsAsync(chatId, userState, cancellationToken);
                        break;
                    case "duel_maps_done":
                        await FinalizeMapSelectionAsync(chatId, userState, callbackQuery, cancellationToken);
                        break;
                    case "duel_step_back":
                        await HandleStepBackAsync(chatId, userState, cancellationToken);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling duel callback: {Data}", data);
                await _duelUIService.ShowErrorAsync(chatId, "An error occurred", userState.Language, cancellationToken);
            }
        }

        private async Task StartCreateDuelAsync(long chatId, UserState userState, CancellationToken cancellationToken)
        {
            var duelState = new CreateDuelState { CurrentStep = 1 };
            await _duelService.UpdateDuelStateAsync(userState.TelegramId, BotState.CreateDuel, duelState);
            await _duelUIService.ShowFormatSelectionAsync(chatId, userState.Language, cancellationToken);
        }

        private async Task HandleFormatSelectionAsync(long chatId, UserState userState, string data, CancellationToken cancellationToken)
        {
            var format = data.Split('_')[2];
            var duelState = await _duelService.GetDuelStateAsync(userState.TelegramId);
            if (!_duelService.IsValidStep(duelState, 1)) return;
            duelState!.Format = format;
            duelState.CurrentStep = 2;
            await _duelService.UpdateDuelStateAsync(userState.TelegramId, BotState.CreateDuelEntryFee, duelState);
            await _duelUIService.ShowEntryFeeStepAsync(chatId, userState.Language, cancellationToken);
        }

        private async Task HandleRoundFormatSelectionAsync(long chatId, UserState userState, string data, CancellationToken cancellationToken)
        {
            var roundFormat = data.Split('_')[2];
            var duelState = await _duelService.GetDuelStateAsync(userState.TelegramId);
            if (!_duelService.IsValidStep(duelState, 3)) return;
            duelState!.RoundFormat = roundFormat;
            duelState.CurrentStep = 4;
            await _duelService.UpdateDuelStateAsync(userState.TelegramId, BotState.CreateDuelPrime, duelState);
            await _duelUIService.ShowPrimeSelectionAsync(chatId, userState.Language, cancellationToken);
        }

        private async Task HandlePrimeSelectionAsync(long chatId, UserState userState, string data, CancellationToken cancellationToken)
        {
            var duelState = await _duelService.GetDuelStateAsync(userState.TelegramId);
            if (!_duelService.IsValidStep(duelState, 4)) return;
            duelState!.PrimeOnly = data == "duel_prime_yes";
            duelState.CurrentStep = 5;
            await _duelService.UpdateDuelStateAsync(userState.TelegramId, BotState.CreateDuelWarmup, duelState);
            await _duelUIService.ShowWarmupSelectionAsync(chatId, userState.Language, cancellationToken);
        }

        private async Task HandleWarmupSelectionAsync(long chatId, UserState userState, string data, CancellationToken cancellationToken)
        {
            var duelState = await _duelService.GetDuelStateAsync(userState.TelegramId);
            if (!_duelService.IsValidStep(duelState, 5)) return;
            if (int.TryParse(data.Split('_')[2], out var minutes))
            {
                duelState!.WarmupMinutes = minutes;
            }
            duelState!.CurrentStep = 6;
            await _duelService.UpdateDuelStateAsync(userState.TelegramId, BotState.CreateDuelMaxRounds, duelState);
            await _duelUIService.ShowMaxRoundsSelectionAsync(chatId, userState.Language, cancellationToken);
        }

        private async Task HandleMaxRoundsSelectionAsync(long chatId, UserState userState, string data, CancellationToken cancellationToken)
        {
            var duelState = await _duelService.GetDuelStateAsync(userState.TelegramId);
            if (!_duelService.IsValidStep(duelState, 6)) return;
            if (int.TryParse(data.Split('_')[2], out var rounds))
            {
                duelState!.MaxRounds = rounds;
            }
            duelState!.CurrentStep = 7;
            await _duelService.UpdateDuelStateAsync(userState.TelegramId, BotState.CreateDuelMaps, duelState);
            await _duelUIService.ShowMapSelectionAsync(chatId, duelState, userState.Language, cancellationToken);
        }

        private async Task HandleMapSelectionAsync(long chatId, UserState userState, string data, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var mapName = data.Replace("duel_map_", "");
            var duelState = await _duelService.GetDuelStateAsync(userState.TelegramId);
            if (!_duelService.IsValidStep(duelState, 7)) return;
            var maxMaps = duelState!.RoundFormat switch
            {
                "bo5" => 5,
                "bo3" => 3,
                _ => 1
            };
            if (duelState.Maps.Contains(mapName))
            {
                duelState.Maps.Remove(mapName);
            }
            else
            {
                if (duelState.Maps.Count >= maxMaps)
                {
                    await _botClient.AnswerCallbackQuery(callbackQuery.Id, text: $"Max {maxMaps} maps", cancellationToken: cancellationToken);
                    return;
                }
                if (maxMaps == 1)
                {
                    duelState.Maps.Clear();
                }
                duelState.Maps.Add(mapName);
            }
            await _duelService.UpdateDuelStateAsync(userState.TelegramId, BotState.CreateDuelMaps, duelState);
            if (maxMaps == 1 && duelState.Maps.Any())
            {
                await FinalizeMapSelectionAsync(chatId, userState, callbackQuery, cancellationToken);
                return;
            }
            await _duelUIService.UpdateMapSelectionAsync(chatId, callbackQuery.Message!.MessageId, duelState, userState.Language, cancellationToken);
            await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
        }

        private async Task FinalizeMapSelectionAsync(long chatId, UserState userState, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var duelState = await _duelService.GetDuelStateAsync(userState.TelegramId);
            if (duelState == null) return;
            await _duelService.UpdateDuelStateAsync(userState.TelegramId, BotState.CreateDuelConfirm, duelState);
            await _duelUIService.ShowDuelConfirmationAsync(chatId, callbackQuery.Message!.MessageId, duelState, userState.Language, cancellationToken);
        }

        private async Task ConfirmDuelCreationAsync(long chatId, UserState userState, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var duelState = await _duelService.GetDuelStateAsync(userState.TelegramId);
            if (_duelService.IsValidStep(duelState, 7) && userState.UserId.HasValue)
            {
                var result = await _duelService.CreateDuelAsync(duelState!, userState.UserId.Value);
                if (result?.Success == true)
                {
                    var duel = result.Data!;
                    var invite = await _duelService.GenerateInviteLinkAsync(duel.Id);
                    var inviteCode = string.Empty;
                    if (invite?.Success == true && !string.IsNullOrEmpty(invite.Data))
                    {
                        inviteCode = invite.Data.Split('/').Last();
                    }
                    await _duelUIService.ShowDuelCreatedAsync(chatId, callbackQuery.Message!.MessageId, duel, inviteCode, userState.Language, cancellationToken);
                    await _userStateService.UpdateUserStateAsync(userState.TelegramId, BotState.MainMenu);
                    await _userStateService.ClearStateDataAsync(userState.TelegramId);
                }
                else
                {
                    var msg = !string.IsNullOrEmpty(result?.Message) ? result!.Message : string.Join(", ", result?.Errors ?? new List<string>());
                    await _duelUIService.ShowErrorAsync(chatId, msg, userState.Language, cancellationToken);
                }
            }
        }

        private async Task ShowAvailableDuelsAsync(long chatId, UserState userState, CancellationToken cancellationToken)
        {
            var duels = await _duelService.GetAvailableDuelsAsync();
            if (duels?.Success == true && duels.Data?.Any() == true)
                await _duelUIService.ShowAvailableDuelsAsync(chatId, duels.Data, userState.Language, cancellationToken);
            else
                await _duelUIService.ShowNoDataAsync(chatId, userState.Language, cancellationToken);
        }

        private async Task ShowMyDuelsAsync(long chatId, UserState userState, CancellationToken cancellationToken)
        {
            if (!userState.UserId.HasValue) return;
            var duels = await _duelService.GetUserDuelsAsync(userState.UserId.Value);
            if (duels?.Success == true && duels.Data?.Any() == true)
                await _duelUIService.ShowMyDuelsAsync(chatId, duels.Data, userState.UserId, userState.Language, cancellationToken);
            else
                await _duelUIService.ShowNoDataAsync(chatId, userState.Language, cancellationToken);
        }

        private async Task JoinDuelAsync(long chatId, UserState userState, string data, CancellationToken cancellationToken)
        {
            if (!userState.UserId.HasValue) return;
            if (!Guid.TryParse(data.Split('_')[2], out var duelId)) return;
            var result = await _duelService.JoinDuelAsync(duelId, userState.UserId.Value);
            if (result?.Success == true)
            {
                await _duelUIService.ShowSuccessAsync(chatId, "duel_joined", userState.Language, cancellationToken);
                await ShowMyDuelsAsync(chatId, userState, cancellationToken);
                if (result.Data != null)
                    await _duelUIService.ShowDuelPaymentInfoAsync(chatId, result.Data, userState.UserId, userState.Language, cancellationToken);
            }
            else
            {
                var msg = !string.IsNullOrEmpty(result?.Message) ? result!.Message : string.Join(", ", result?.Errors ?? new List<string>());
                await _duelUIService.ShowErrorAsync(chatId, msg, userState.Language, cancellationToken);
            }
        }

        private static Task ViewDuelDetailsAsync(long chatId, UserState userState, string data, CancellationToken cancellationToken)
        {
            // Placeholder for future implementation
            return Task.CompletedTask;
        }

        private async Task HandleGetServerAsync(long chatId, UserState userState, string data, CancellationToken cancellationToken)
        {
            if (!_adminService.IsAdmin(chatId))
                return;

            if (!Guid.TryParse(data.Split('_')[3], out var duelId)) return;
            var duel = await _duelService.GetFullDuelAsync(duelId);
            if (duel?.Success == true && duel.Data != null)
                await _duelUIService.ShowGetServerAsync(chatId, duel.Data, userState.Language, cancellationToken);
        }

        private async Task HandleReadyAsync(long chatId, UserState userState, string data, CancellationToken cancellationToken)
        {
            if (!userState.UserId.HasValue || !Guid.TryParse(data.Split('_')[2], out var duelId))
                return;

            var result = await _duelService.ConfirmReadyAsync(duelId, userState.UserId.Value);
            if (result?.Success == true)
            {
                if (result?.Data.HasPaid == false)
                    await _paymentUIService.ShowWaitingOpponentAsync(chatId, userState.Language, cancellationToken);
                else if (result.Data.IsReady == false)
                    await _paymentUIService.ShowWaitingOpponentReadyAsync(chatId, userState.Language, cancellationToken);
              
            }
            else if (result?.Success == false)
            {
                await _duelUIService.ShowErrorAsync(chatId, string.Join(", ", result.Errors ?? []), userState.Language, cancellationToken);
            }
        }

        private async Task HandleStepBackAsync(long chatId, UserState userState, CancellationToken cancellationToken)
        {
            var duelState = await _duelService.GetDuelStateAsync(userState.TelegramId);
            if (duelState == null) return;
            switch (duelState.CurrentStep)
            {
                case 2:
                    await _duelUIService.ShowFormatSelectionAsync(chatId, userState.Language, cancellationToken);
                    duelState.CurrentStep = 1;
                    await _duelService.UpdateDuelStateAsync(userState.TelegramId, BotState.CreateDuel, duelState);
                    break;
                case 3:
                    await _duelUIService.ShowEntryFeeStepAsync(chatId, userState.Language, cancellationToken);
                    duelState.CurrentStep = 2;
                    await _duelService.UpdateDuelStateAsync(userState.TelegramId, BotState.CreateDuelEntryFee, duelState);
                    break;
                case 4:
                    await _duelUIService.ShowRoundFormatSelectionAsync(chatId, userState.Language, cancellationToken);
                    duelState.CurrentStep = 3;
                    await _duelService.UpdateDuelStateAsync(userState.TelegramId, BotState.CreateDuelRounds, duelState);
                    break;
                case 5:
                    await _duelUIService.ShowPrimeSelectionAsync(chatId, userState.Language, cancellationToken);
                    duelState.CurrentStep = 4;
                    await _duelService.UpdateDuelStateAsync(userState.TelegramId, BotState.CreateDuelPrime, duelState);
                    break;
                case 6:
                    await _duelUIService.ShowWarmupSelectionAsync(chatId, userState.Language, cancellationToken);
                    duelState.CurrentStep = 5;
                    await _duelService.UpdateDuelStateAsync(userState.TelegramId, BotState.CreateDuelWarmup, duelState);
                    break;
                case 7:
                    await _duelUIService.ShowMaxRoundsSelectionAsync(chatId, userState.Language, cancellationToken);
                    duelState.CurrentStep = 6;
                    await _duelService.UpdateDuelStateAsync(userState.TelegramId, BotState.CreateDuelMaxRounds, duelState);
                    break;
            }
        }

        private async Task HandleForfeitInitiateAsync(long chatId, UserState userState, string data, CancellationToken ct)
        {
            if (!Guid.TryParse(data.Split('_')[2], out var duelId)) return;
            await _duelUIService.ShowForfeitConfirmationAsync(chatId, duelId, userState.Language, ct);
        }

        private async Task HandleForfeitConfirmAsync(long chatId, UserState userState, string data, CancellationToken ct)
        {
            if (!userState.UserId.HasValue || !Guid.TryParse(data.Split('_')[3], out var duelId)) return;

            var result = await _duelService.ForfeitDuelAsync(duelId, userState.UserId.Value, ForfeitReason.Confirmed);
            if (result?.Success == true)
            {
                await _duelUIService.ShowForfeitSuccessAsync(chatId, result.Data!, userState.Language, ct);
            }
            else
            {
                await _duelUIService.ShowErrorAsync(chatId, result?.Message ?? "Помилка", userState.Language, ct);
            }
        }

        private async Task HandleForfeitBadPingAsync(long chatId, UserState userState, string data, CancellationToken ct)
        {
            if (!userState.UserId.HasValue || !Guid.TryParse(data.Split('_')[3], out var duelId)) return;

            await _duelUIService.ShowPingCheckingAsync(chatId, userState.Language, ct);

            var result = await _duelService.ForfeitDuelAsync(duelId, userState.UserId.Value, ForfeitReason.BadPing);
            if (result?.Success == true)
            {
                if (result.Data!.RefundIssued)
                {
                    await _duelUIService.ShowForfeitBadPingRefundAsync(chatId, result.Data, userState.Language, ct);
                }
                else
                {
                    await _duelUIService.ShowForfeitPingNormalDefeatAsync(chatId, result.Data, userState.Language, ct);
                }
            }
            else
            {
                await _duelUIService.ShowErrorAsync(chatId, result?.Message ?? "Помилка", userState.Language, ct);
            }
        }
    }
}
