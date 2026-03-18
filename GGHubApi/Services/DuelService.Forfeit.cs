using GGHubApi.Hubs;
using GGHubDb.Models;
using GGHubDb.Repos;
using GGHubDb.Services;
using GGHubShared.Enums;
using GGHubShared.Models;
using Microsoft.Extensions.Logging;

namespace GGHubApi.Services
{
    public partial class DuelService
    {
        public async Task<ApiResponse<DuelForfeitResult>> ForfeitDuelAsync(
            Guid duelId,
            Guid userId,
            ForfeitReason reason,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("User {UserId} forfeiting duel {DuelId} with reason {Reason}", userId, duelId, reason);

                // 1. Отримати дуель
                var duel = await _duelRepository.GetFullDuelAsync(duelId, ct);
                if (duel == null)
                {
                    return new ApiResponse<DuelForfeitResult>
                    {
                        Success = false,
                        Code = ErrorCode.NotFound,
                        Message = "Дуель не знайдено"
                    };
                }

                // 2. Перевірити статус дуелі
                if (duel.Status != DuelStatus.InProgress &&
                    duel.Status != DuelStatus.Starting &&
                    duel.Status != DuelStatus.WaitingForLaunch)
                {
                    return new ApiResponse<DuelForfeitResult>
                    {
                        Success = false,
                        Code = ErrorCode.ValidationFailed,
                        Message = "Можна покинути тільки активну дуель"
                    };
                }

                // 3. Перевірити що користувач - учасник
                var participant = duel.Participants.FirstOrDefault(p => p.UserId == userId);
                if (participant == null)
                {
                    return new ApiResponse<DuelForfeitResult>
                    {
                        Success = false,
                        Code = ErrorCode.ValidationFailed,
                        Message = "Ви не є учасником цієї дуелі"
                    };
                }

                // 4. Знайти опонента
                var opponent = duel.Participants.FirstOrDefault(p => p.UserId != userId);

                var result = new DuelForfeitResult();

                // 5. Обробити згідно причини
                if (reason == ForfeitReason.BadPing && duel.GameServer != null)
                {
                    // АВТОМАТИЧНА ПЕРЕВІРКА ПІНГУ
                    var pingAnalysis = await _pingAnalysisService.AnalyzePlayerPingAsync(
                        duel.GameServer.ExternalServerId!,
                        participant.User.Username ?? participant.User.SteamId ?? "Unknown",
                        participant.User.SteamId!,
                        ct);

                    // Логування для адміністратора
                    await LogForfeitAsync(duel.Id, userId, reason, pingAnalysis, ct);

                    if (!pingAnalysis.IsPingAcceptable)
                    {
                        // Пінг дійсно поганий - повернути кошти обом
                        await _transactionService.RefundDuelEntryFeesAsync(duelId,
                            $"Автоматичне повернення: поганий пінг ({pingAnalysis.AveragePing}ms)", ct);

                        duel.Status = DuelStatus.Cancelled;
                        result.RefundIssued = true;
                        result.MeasuredPing = pingAnalysis.AveragePing;
                        result.Message = $"Підтверджено поганий пінг ({pingAnalysis.AveragePing}ms). Кошти повернуті обом гравцям.";
                    }
                    else
                    {
                        // Пінг нормальний - технічна поразка
                        await CompleteDuelWithTechnicalDefeatAsync(duel, participant, opponent, ct);
                        result.WinnerId = opponent?.UserId;
                        result.MeasuredPing = pingAnalysis.AveragePing;
                        result.Message = $"Пінг в нормі ({pingAnalysis.AveragePing}ms). Зараховано технічну поразку.";
                    }
                }
                else
                {
                    // ПІДТВЕРДЖЕНИЙ ВИХІД - технічна поразка
                    await CompleteDuelWithTechnicalDefeatAsync(duel, participant, opponent, ct);
                    result.WinnerId = opponent?.UserId;
                    result.Message = "Технічну поразку зараховано.";

                    await LogForfeitAsync(duel.Id, userId, reason, null, ct);
                }

                // 6. Зберегти зміни в БД
                await _context.SaveChangesAsync(ct);

                // 7. Зупинити сервер
                if (duel.GameServer?.ExternalServerId != null)
                {
                    await _dathostService.StopServerAsync(duel.GameServer.ExternalServerId);
                }

                // 8. Сповістити опонента через SignalR
                if (opponent != null)
                {
                    await _duelHubService.NotifyDuelForfeited(duelId, userId, opponent.UserId, result);
                }

                result.Forfeited = true;
                return new ApiResponse<DuelForfeitResult>
                {
                    Success = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forfeiting duel {DuelId} by user {UserId}", duelId, userId);
                return new ApiResponse<DuelForfeitResult>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Message = "Помилка при покиданні дуелі",
                    Errors = { ex.Message }
                };
            }
        }

        private async Task CompleteDuelWithTechnicalDefeatAsync(
            Duel duel,
            DuelParticipant forfeitedPlayer,
            DuelParticipant? winner,
            CancellationToken ct)
        {
            forfeitedPlayer.TechnicalDefeat = true;
            forfeitedPlayer.ForfeitReason = "Гравець покинув матч";

            if (winner != null)
            {
                duel.WinnerId = winner.UserId;
                duel.Status = DuelStatus.Completed;
                duel.CompletedAt = DateTime.UtcNow;

                // Виплата призового фонду
                await _transactionService.CreatePrizeAsync(
                    winner.UserId,
                    duel.Id,
                    duel.PrizeFund,
                    ct);
            }
            else
            {
                duel.Status = DuelStatus.Cancelled;
            }
        }

        private async Task LogForfeitAsync(
            Guid duelId,
            Guid userId,
            ForfeitReason reason,
            PingAnalysisResult? pingAnalysis,
            CancellationToken ct)
        {
            var log = new DuelForfeitLog
            {
                DuelId = duelId,
                UserId = userId,
                Reason = reason,
                MeasuredPing = pingAnalysis?.AveragePing,
                PacketLossIn = pingAnalysis?.PacketLossIn,
                PacketLossOut = pingAnalysis?.PacketLossOut,
                AutoRefunded = pingAnalysis?.IsPingAcceptable == false,
                AdminReviewed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _context.DuelForfeitLogs.AddAsync(log, ct);
        }
    }
}
