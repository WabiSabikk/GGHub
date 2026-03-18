using AutoMapper;
using GGHubApi.Hubs;
using GGHubDb;
using GGHubDb.Models;
using GGHubDb.Repos;
using GGHubDb.Services;
using GGHubShared.Enums;
using GGHubShared.Models;

namespace GGHubApi.Services
{
    public interface IDuelService
    {
        Task<ApiResponse<DuelDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<DuelDto>> GetFullDuelAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<DuelDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<DuelDto>>> GetPagedWithFiltersAsync(
            int pageNumber,
            int pageSize,
            DuelFormat? format = null,
            DuelStatus? status = null,
            Guid? userId = null,
            CancellationToken cancellationToken = default);
        Task<ApiResponse<List<DuelDto>>> GetAvailableDuelsAsync(CancellationToken cancellationToken = default);
        Task<ApiResponse<List<DuelDto>>> GetUserDuelsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<DuelDto>> CreateDuelAsync(CreateDuelRequest request, Guid createdBy, CancellationToken cancellationToken = default);
        Task<ApiResponse<DuelDto>> JoinDuelAsync(JoinDuelRequest request, Guid userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> LeaveDuelAsync(Guid duelId, Guid userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>>                          StartDuelAsync(Guid duelId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> CompleteDuelAsync(Guid duelId, Guid winnerId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ProcessMatchResultAsync(Guid duelId, DathostMatchResult result, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> CancelDuelAsync(Guid duelId, Guid userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> UpdateStatusAsync(Guid duelId, DuelStatus status, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> GenerateInviteLinkAsync(Guid duelId, CancellationToken cancellationToken = default);
        Task<ApiResponse<DuelDto>> JoinByInviteLinkAsync(string inviteCode, Guid userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<DuelDto>> PayEntryFeeAsync(Guid duelId, Guid userId, PaymentProvider paymentProvider, CancellationToken cancellationToken = default);
        Task<ApiResponse<DuelDto>> MarkEntryFeePaidAsync(Guid duelId, Guid userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<OpponentStatus>> ConfirmReadyAsync(Guid duelId, Guid userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<DuelForfeitResult>> ForfeitDuelAsync(Guid duelId, Guid userId, ForfeitReason reason, CancellationToken cancellationToken = default);
    }
    public partial class DuelService : IDuelService
    {
        private readonly ICryptomusService _cryptomusService;
        private readonly IDuelRepository _duelRepository;
        private readonly IUserRepository _userRepository;
        private readonly IServerService _serverService;
        private readonly IDathostService _dathostService;
        private readonly IMatchConfigService _matchConfigService;
        private readonly ITransactionService _transactionService;
        private readonly IDuelHubService _duelHubService;
        private readonly IMapper _mapper;
        private readonly ILogger<DuelService> _logger;
        private readonly string _publicBaseUrl;
        private readonly IPingAnalysisService _pingAnalysisService;
        private readonly AppDbContext _context;

        public DuelService(
            IDuelRepository duelRepository,
            IUserRepository userRepository,
            IDathostService dathostService,
            IMatchConfigService matchConfigService,
            ITransactionService transactionService,
            IMapper mapper,
            ILogger<DuelService> logger,
            IConfiguration configuration,
            ICryptomusService cryptomusService,
            IServerService regionService,
            IDuelHubService duelHubService,
            IPingAnalysisService pingAnalysisService,
            AppDbContext context)
        {
            _duelRepository = duelRepository;
            _userRepository = userRepository;
            _dathostService = dathostService;
            _matchConfigService = matchConfigService;
            _transactionService = transactionService;
            _mapper = mapper;
            _logger = logger;
            _publicBaseUrl = configuration["App:PublicBaseUrl"] ?? string.Empty;
            _cryptomusService = cryptomusService;
            _duelHubService = duelHubService;
            _serverService = regionService;
            _pingAnalysisService = pingAnalysisService;
            _context = context;
        }

        public async Task<ApiResponse<DuelDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var duel = await _duelRepository.GetWithParticipantsAsync(id, cancellationToken);
                if (duel == null)
                {
                    return new ApiResponse<DuelDto>
                    {
                        Success = false,
                        Code = ErrorCode.NotFound
                    };
                }

                return new ApiResponse<DuelDto>
                {
                    Success = true,
                    Data = _mapper.Map<DuelDto>(duel)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting duel by ID: {DuelId}", id);
                return new ApiResponse<DuelDto>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<DuelDto>> GetFullDuelAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var duel = await _duelRepository.GetFullDuelAsync(id, cancellationToken);
                if (duel == null)
                {
                    return new ApiResponse<DuelDto>
                    {
                        Success = false,
                        Code = ErrorCode.NotFound
                    };
                }

                return new ApiResponse<DuelDto>
                {
                    Success = true,
                    Data = _mapper.Map<DuelDto>(duel)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting full duel data: {DuelId}", id);
                return new ApiResponse<DuelDto>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<PagedResult<DuelDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            try
            {
                var (duels, totalCount) = await _duelRepository.GetPagedAsync(pageNumber, pageSize, cancellationToken: cancellationToken);
                var duelDtos = _mapper.Map<List<DuelDto>>(duels);

                return new ApiResponse<PagedResult<DuelDto>>
                {
                    Success = true,
                    Data = new PagedResult<DuelDto>
                    {
                        Items = duelDtos,
                        TotalCount = totalCount,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged duels");
                return new ApiResponse<PagedResult<DuelDto>>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<PagedResult<DuelDto>>> GetPagedWithFiltersAsync(
            int pageNumber,
            int pageSize,
            DuelFormat? format = null,
            DuelStatus? status = null,
            Guid? userId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var (duels, totalCount) = await _duelRepository.GetPagedWithFiltersAsync(
                    pageNumber, pageSize, format, status, userId, cancellationToken);

                var duelDtos = _mapper.Map<List<DuelDto>>(duels);

                return new ApiResponse<PagedResult<DuelDto>>
                {
                    Success = true,
                    Data = new PagedResult<DuelDto>
                    {
                        Items = duelDtos,
                        TotalCount = totalCount,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filtered duels");
                return new ApiResponse<PagedResult<DuelDto>>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<List<DuelDto>>> GetAvailableDuelsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var duels = await _duelRepository.GetAvailableDuelsAsync(cancellationToken);
                var duelDtos = _mapper.Map<List<DuelDto>>(duels);

                return new ApiResponse<List<DuelDto>>
                {
                    Success = true,
                    Data = duelDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available duels");
                return new ApiResponse<List<DuelDto>>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<List<DuelDto>>> GetUserDuelsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var duels = await _duelRepository.GetByUserIdAsync(userId, cancellationToken);
                var duelDtos = _mapper.Map<List<DuelDto>>(duels);

                return new ApiResponse<List<DuelDto>>
                {
                    Success = true,
                    Data = duelDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user duels: {UserId}", userId);
                return new ApiResponse<List<DuelDto>>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<DuelDto>> CreateDuelAsync(CreateDuelRequest request, Guid createdBy, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate user
                var user = await _userRepository.GetByIdAsync(createdBy, cancellationToken);
                if (user == null)
                {
                    return new ApiResponse<DuelDto>
                    {
                        Success = false,
                        Code = ErrorCode.UserNotFound
                    };
                }

                //if (!user.IsPrimeActive)
                //{
                //    return new ApiResponse<DuelDto>
                //    {
                //        Success = false,
                //        Message = "Prime subscription required to create duels"
                //    };
                //}

                // Check active duels limit
                var activeDuelsCount = await _duelRepository.GetUserActiveDuelsCountAsync(createdBy, cancellationToken);
                if (activeDuelsCount >= 3)
                {
                    return new ApiResponse<DuelDto>
                    {
                        Success = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                var validation = await _matchConfigService.ValidateCustomSettingsAsync(request.Format, request, cancellationToken);
                if (!validation.Success)
                {
                    return new ApiResponse<DuelDto>
                    {
                        Success = false,
                        Code = validation.Code,
                        Message = validation.Message
                    };
                }

                // Calculate prize fund and commission
                var maxParticipants = (int)request.Format * 2;
                var commission = request.EntryFee * maxParticipants * 0.10m;
                var prizeFund = (request.EntryFee * maxParticipants) - commission;

                // Create duel entity
                var duel = new Duel
                {
                    Title = request.Title,
                    Format = request.Format,
                    RoundFormat = request.RoundFormat,
                    EntryFee = request.EntryFee,
                    PrimeOnly = request.PrimeOnly,
                    WarmupMinutes = request.WarmupMinutes,
                    CustomMaxRounds = request.CustomMaxRounds,
                    CustomTickrate = request.CustomTickrate,
                    CustomOvertimeEnabled = request.CustomOvertimeEnabled,
                    PreferredRegion = request.PreferredRegion,
                    PrizeFund = prizeFund,
                    Commission = commission,
                    MaxParticipants = maxParticipants,
                    CurrentParticipants = 1,
                    CreatedBy = createdBy,
                    Status = DuelStatus.WaitingForPlayers
                };

                // Create duel with maps and participant using repository
                var createdDuel = await _duelRepository.CreateDuelWithMapsAndParticipantAsync(
                    duel, request.Maps, createdBy, cancellationToken);

                _logger.LogInformation("Created duel: {DuelId} by user: {UserId}", createdDuel.Id, createdBy);

                return new ApiResponse<DuelDto>
                {
                    Success = true,
                    Data = _mapper.Map<DuelDto>(createdDuel),
                    Message = "Duel created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating duel for user: {UserId}", createdBy);
                return new ApiResponse<DuelDto>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<DuelDto>> JoinDuelAsync(JoinDuelRequest request, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate user
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    return new ApiResponse<DuelDto>
                    {
                        Success = false,
                        Code = ErrorCode.UserNotFound
                    };
                }


                // Get duel with participants
                var duel = await _duelRepository.GetWithParticipantsAsync(request.DuelId, cancellationToken);
                if (duel == null)
                {
                    return new ApiResponse<DuelDto>
                    {
                        Success = false,
                        Code = ErrorCode.NotFound
                    };
                }

                //if (duel.PrimeOnly && !user.IsPrimeActive) prime subscription in bot is not cs2 prime
                //{
                //    return new ApiResponse<DuelDto>
                //    {
                //        Success = false,
                //        Message = "Prime subscription required"
                //    };
                //}

                // Validate duel state
                if (duel.Status != DuelStatus.WaitingForPlayers)
                {
                    return new ApiResponse<DuelDto>
                    {
                        Success = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                if (duel.CurrentParticipants >= duel.MaxParticipants)
                {
                    return new ApiResponse<DuelDto>
                    {
                        Success = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                if (await _duelRepository.IsUserInDuelAsync(userId, request.DuelId, cancellationToken))
                {
                    return new ApiResponse<DuelDto>
                    {
                        Success = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                // Determine team
                var team = request.Team ?? GetNextAvailableTeam(duel);

                // Join duel using repository
                var updatedDuel = await _duelRepository.AddParticipantToDuelAsync(
                    request.DuelId, userId, team, cancellationToken);

                await _duelHubService.AddUserToDuelGroup(request.DuelId, userId);

                _logger.LogInformation("User {UserId} joined duel: {DuelId}", userId, request.DuelId);

                return new ApiResponse<DuelDto>
                {
                    Success = true,
                    Data = _mapper.Map<DuelDto>(updatedDuel),
                    Message = "Successfully joined duel"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining duel {DuelId} for user: {UserId}", request.DuelId, userId);
                return new ApiResponse<DuelDto>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> LeaveDuelAsync(Guid duelId, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var duel = await _duelRepository.GetFullDuelAsync(duelId, cancellationToken);
                if (duel == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.NotFound
                    };
                }

                // Validate duel state
                if (duel.Status != DuelStatus.WaitingForPlayers && duel.Status != DuelStatus.PaymentPending)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                var participant = duel.Participants.FirstOrDefault(p => p.UserId == userId);
                if (participant == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                // Remove participant using repository
                await _duelRepository.RemoveParticipantFromDuelAsync(duelId, userId, cancellationToken);

                _logger.LogInformation("User {UserId} left duel: {DuelId}", userId, duelId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Successfully left duel"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving duel {DuelId} for user: {UserId}", duelId, userId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> StartDuelAsync(Guid duelId, CancellationToken cancellationToken = default)
        {
            try
            {
                await _duelRepository.UpdateStatusAsync(duelId, DuelStatus.Starting, cancellationToken);
                _logger.LogInformation("Started duel: {DuelId}", duelId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Duel started successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting duel: {DuelId}", duelId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> CompleteDuelAsync(Guid duelId, Guid winnerId, CancellationToken cancellationToken = default)
        {
            try
            {
                await _duelRepository.CompleteDuelAsync(duelId, winnerId, cancellationToken);
                _logger.LogInformation("Completed duel: {DuelId} with winner: {WinnerId}", duelId, winnerId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Duel completed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing duel: {DuelId}", duelId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> ProcessMatchResultAsync(Guid duelId, DathostMatchResult result, CancellationToken cancellationToken = default)
        {
            try
            {
                var duel = await _duelRepository.GetFullDuelAsync(duelId, cancellationToken);
                if (duel == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.NotFound
                    };
                }

                var winnerTeam = result.WinnerTeam == "team_a" ? 1 : 2;
                var winner = duel.Participants.FirstOrDefault(p => p.Team == winnerTeam);
                if (winner == null)
                {
                    _logger.LogWarning("Winner not found for duel {DuelId} in DatHost result", duelId);
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.NotFound
                    };
                }


                await _duelRepository.CompleteDuelAsync(duelId, winner.UserId, cancellationToken);

                if (!string.IsNullOrEmpty(duel.GameServer?.ExternalServerId))
                {
                    await _dathostService.StopServerAsync(duel.GameServer.ExternalServerId);
                }

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Match result processed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing duel result: {DuelId}", duelId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> CancelDuelAsync(Guid duelId, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var duel = await _duelRepository.GetByIdAsync(duelId, cancellationToken);
                if (duel == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.NotFound
                    };
                }

                if (duel.CreatedBy != userId)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                if (duel.Status == DuelStatus.InProgress || duel.Status == DuelStatus.Completed)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                await _duelRepository.UpdateStatusAsync(duelId, DuelStatus.Cancelled, cancellationToken);
                _logger.LogInformation("Cancelled duel: {DuelId} by user: {UserId}", duelId, userId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Duel cancelled successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling duel: {DuelId}", duelId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateStatusAsync(Guid duelId, DuelStatus status, CancellationToken cancellationToken = default)
        {
            try
            {
                await _duelRepository.UpdateStatusAsync(duelId, status, cancellationToken);
                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Duel status updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating duel status: {DuelId} to {Status}", duelId, status);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<string>> GenerateInviteLinkAsync(Guid duelId, CancellationToken cancellationToken = default)
        {
            try
            {
                var inviteCode = await _duelRepository.GenerateInviteLinkAsync(duelId, cancellationToken);
                if (inviteCode == null)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Code = ErrorCode.NotFound
                    };
                }

                var baseUrl = _publicBaseUrl.EndsWith('/') ? _publicBaseUrl : _publicBaseUrl + "/";
                var fullLink = $"{baseUrl}api/duels/join-by-link/{inviteCode}";

                return new ApiResponse<string>
                {
                    Success = true,
                    Data = fullLink,
                    Message = "Invite link generated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invite link for duel: {DuelId}", duelId);
                return new ApiResponse<string>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<DuelDto>> JoinByInviteLinkAsync(string inviteCode, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var duel = await _duelRepository.FirstOrDefaultAsync(d => d.InviteLink == inviteCode, cancellationToken);
                if (duel == null)
                {
                    return new ApiResponse<DuelDto>
                    {
                        Success = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                var joinRequest = new JoinDuelRequest { DuelId = duel.Id };
                var result = await JoinDuelAsync(joinRequest, userId, cancellationToken);

                if (result.Success && result.Data != null)
                {
                    var participantIds = result.Data.Participants.Select(p => p.User.Id);
                    await _duelHubService.NotifyOpponentJoined(duel.Id, userId, participantIds);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining duel by invite code: {InviteCode}", inviteCode);
                return new ApiResponse<DuelDto>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<DuelDto>> PayEntryFeeAsync(Guid duelId, Guid userId, PaymentProvider paymentProvider, CancellationToken cancellationToken = default)
        {
            try
            {
                var duel = await _duelRepository.GetFullDuelAsync(duelId, cancellationToken);
                if (duel == null)
                {
                    return new ApiResponse<DuelDto> { Success = false, Code = ErrorCode.NotFound };
                }

                var participant = duel.Participants.FirstOrDefault(p => p.UserId == userId);
                if (participant == null)
                {
                    return new ApiResponse<DuelDto> { Success = false, Code = ErrorCode.ValidationFailed };
                }

                if (participant.HasPaid)
                {
                    return new ApiResponse<DuelDto> { Success = false, Code = ErrorCode.PaymentError };
                }

                if (paymentProvider == PaymentProvider.Manual)
                {
                    var payment = await _transactionService.CreateEntryFeeAsync(userId, duelId, duel.EntryFee, cancellationToken);
                    if (!payment.Success)
                    {
                        return new ApiResponse<DuelDto>
                        {
                            Success = false,
                            Code = payment.Code,
                            Errors = payment.Errors
                        };
                    }

                    await _duelRepository.SetParticipantPaidAsync(duelId, userId, true, cancellationToken);
                }
                else if (paymentProvider == PaymentProvider.Cryptomus)
                {
                    var txs = await _transactionService.GetByDuelIdAsync(duelId, cancellationToken);
                    var pending = txs.Success ? txs.Data?
                        .Where(t => t.UserId == userId && t.Type == TransactionType.Deposit && t.Status == TransactionStatus.Pending)
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault() : null;

                    if (pending != null)
                    {
                        if (pending.ExpiresAt.HasValue && pending.ExpiresAt > DateTime.UtcNow && !string.IsNullOrEmpty(pending.PaymentUrl))
                        {
                            return new ApiResponse<DuelDto>
                            {
                                Success = true,
                                Data = _mapper.Map<DuelDto>(duel),
                                Message = pending.PaymentUrl
                            };
                        }

                        await _transactionService.CancelTransactionAsync(pending.Id, cancellationToken);
                    }

                    var baseUrl = _publicBaseUrl.EndsWith('/') ? _publicBaseUrl : _publicBaseUrl + "/";
                    var cryptomusRequest = new CreateCryptomusPaymentRequest
                    {
                        Amount = duel.EntryFee,
                        OrderId = $"duel-{duelId}-{Guid.NewGuid()}",
                        ReturnUrl = $"{baseUrl}payment/success",
                        CallbackUrl = $"{baseUrl}api/webhook/cryptomus"
                    };

                    var cryptoResult = await _cryptomusService.CreatePaymentAsync(cryptomusRequest);
                    if (!cryptoResult.Success)
                    {
                        return new ApiResponse<DuelDto>
                        {
                            Success = false,
                            Code = ErrorCode.PaymentError
                        };
                    }

                    var deposit = await _transactionService.CreateDepositAsync(userId, duel.EntryFee, PaymentProvider.Cryptomus, duel.Id, cancellationToken);
                    if (!deposit.Success || deposit.Data == null)
                    {
                        return new ApiResponse<DuelDto> { Success = false, Code = deposit.Code };
                    }

                    var expiresAt = cryptoResult.ExpiredAt.HasValue
                        ? DateTimeOffset.FromUnixTimeSeconds(cryptoResult.ExpiredAt.Value).UtcDateTime
                        : (DateTime?)null;

                    await _transactionService.UpdateTransactionInfoAsync(
                        deposit.Data.Id,
                        cryptoResult.PaymentId,
                        cryptoResult.PaymentUrl,
                        expiresAt,
                        cancellationToken);

                    var msg = pending != null ? $"expired|{cryptoResult.PaymentUrl}" : cryptoResult.PaymentUrl;

                    return new ApiResponse<DuelDto>
                    {
                        Success = true,
                        Data = _mapper.Map<DuelDto>(duel),
                        Message = msg
                    };
                }


                var allPaid = await _duelRepository.HaveAllParticipantsPaidAsync(duelId, cancellationToken);
                if (allPaid)
                {
                    var map = duel.Maps.OrderBy(m => m.Order).FirstOrDefault()?.MapName ?? "de_dust2";
                    var steamIds = duel.Participants.Select(p => p.User.SteamId).Where(s => !string.IsNullOrEmpty(s)).ToList();

                    var requiredSlots = Math.Max(duel.MaxParticipants + 2, 5);
                    var location = await _serverService.GetOptimalLocationAsync(null, duel.PreferredRegion, cancellationToken);
                    var availableServer = await _serverService.FindAvailableServerAsync(location, requiredSlots, cancellationToken);

                    DathostServerResult server;
                    GameServer gameServerEntity;

                    if (availableServer != null)
                    {
                        _logger.LogInformation("Reusing existing server {ServerId} for duel {DuelId}",
                            availableServer.ExternalServerId, duelId);

                        server = await _dathostService.UpdateServerForDuelAsync(
                            availableServer.ExternalServerId!,
                            duelId,
                            duel.Format,
                            map,
                            steamIds,
                            duel.PrimeOnly,
                            duel.WarmupMinutes,
                            null,
                            duel.CustomMaxRounds,
                            cancellationToken:
                            cancellationToken);

                        if (server.Success)
                        {
                            await _duelRepository.UpdateGameServerForDuelAsync(
                                availableServer.DuelId, duelId, server.Password!, cancellationToken);

                            gameServerEntity = availableServer;
                            gameServerEntity.DuelId = duelId;
                            gameServerEntity.Password = server.Password;
                        }
                        else
                        {
                            availableServer = null;
                        }
                    }

                    if (availableServer == null)
                    {
                        server = await CreateNewServerForDuel(duel, map, steamIds, cancellationToken);
                        if (server.Success)
                        {
                            gameServerEntity = await CreateGameServerEntity(server, duelId, cancellationToken);
                        }
                    }
                    // Сервер буде запущено після підтвердження готовності
                }

                var updated = await _duelRepository.GetFullDuelAsync(duelId, cancellationToken);
                return new ApiResponse<DuelDto>
                {
                    Success = true,
                    Data = _mapper.Map<DuelDto>(updated),
                    Message = allPaid ? "All players paid" : "Entry fee paid"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error paying entry fee for duel {DuelId}", duelId);
                return new ApiResponse<DuelDto>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<DuelDto>> MarkEntryFeePaidAsync(Guid duelId, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var duel = await _duelRepository.GetFullDuelAsync(duelId, cancellationToken);
                if (duel == null)
                {
                    return new ApiResponse<DuelDto> { Success = false, Code = ErrorCode.NotFound };
                }

                var participant = duel.Participants.FirstOrDefault(p => p.UserId == userId);
                if (participant == null)
                {
                    return new ApiResponse<DuelDto> { Success = false, Code = ErrorCode.ValidationFailed };
                }

                if (participant.HasPaid)
                {

                    var allAlreadyPaid = await _duelRepository.HaveAllParticipantsPaidAsync(duelId, cancellationToken);

                    if (!allAlreadyPaid)
                        return new ApiResponse<DuelDto> { Success = true, Code = ErrorCode.PaymentSuccess };

                    if (allAlreadyPaid && duel.GameServer != null)
                    {
                        var updatedPaid = await _duelRepository.GetFullDuelAsync(duelId, cancellationToken);
                        return new ApiResponse<DuelDto>
                        {
                            Success = true,
                            Data = _mapper.Map<DuelDto>(updatedPaid),
                            Code = ErrorCode.PaymentSuccess,
                            Message = allAlreadyPaid ? "All players paid" : "Entry fee paid"
                        };
                    }
                }

                await _duelRepository.SetParticipantPaidAsync(duelId, userId, hasPaid: true, cancellationToken);

                var allPaid = await _duelRepository.HaveAllParticipantsPaidAsync(duelId, cancellationToken);
                if (allPaid)
                {
                    var map = duel.Maps.OrderBy(m => m.Order).FirstOrDefault()?.MapName ?? "de_dust2";
                    var steamIds = duel.Participants.Select(p => p.User.SteamId).Where(s => !string.IsNullOrEmpty(s)).ToList();

                    var requiredSlots = Math.Max(duel.MaxParticipants + 2, 5);
                    var location = await _serverService.GetOptimalLocationAsync(null, duel.PreferredRegion, cancellationToken);
                    var availableServer = await _serverService.FindAvailableServerAsync(location, requiredSlots, cancellationToken);

                    DathostServerResult server;
                    GameServer gameServerEntity;

                    if (availableServer != null)
                    {
                        _logger.LogInformation("Reusing existing server {ServerId} for duel {DuelId}",
                            availableServer.ExternalServerId, duelId);

                        server = await _dathostService.UpdateServerForDuelAsync(
                            availableServer.ExternalServerId!,
                            duelId,
                            duel.Format,
                            map,
                            steamIds,
                            duel.PrimeOnly,
                            duel.WarmupMinutes,
                            null,
                            duel.CustomMaxRounds,
                            cancellationToken:
                            cancellationToken);

                        if (server.Success)
                        {
                            await _duelRepository.UpdateGameServerForDuelAsync(
                                availableServer.DuelId, duelId, server.Password!, cancellationToken);

                            gameServerEntity = availableServer;
                            gameServerEntity.DuelId = duelId;
                            gameServerEntity.Password = server.Password;
                        }
                        else
                        {
                            availableServer = null;
                        }
                    }

                    if (availableServer == null)
                    {
                        server = await CreateNewServerForDuel(duel, map, steamIds, cancellationToken);
                        if (server.Success)
                        {
                            gameServerEntity = await CreateGameServerEntity(server, duelId, cancellationToken);
                        }
                    }

                    await _duelRepository.UpdateStatusAsync(duelId, DuelStatus.WaitingForLaunch);
                }

                var updated = await _duelRepository.GetFullDuelAsync(duelId, cancellationToken);
                return new ApiResponse<DuelDto>
                {
                    Success = true,
                    Data = _mapper.Map<DuelDto>(updated),
                    Message = allPaid ? "All players paid" : "Entry fee paid"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking entry fee paid for duel {DuelId}", duelId);
                return new ApiResponse<DuelDto>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<OpponentStatus>> ConfirmReadyAsync(Guid duelId, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var duel = await _duelRepository.GetFullDuelAsync(duelId, cancellationToken);
                if (duel == null)
                {
                    return new ApiResponse<OpponentStatus> { Success = false, Data = new(), Code = ErrorCode.NotFound };
                }

                if (!duel.Participants.Any(p => p.UserId == userId))
                {
                    return new ApiResponse<OpponentStatus> { Success = false, Data = new(), Code = ErrorCode.ValidationFailed };
                }

                await _duelRepository.SetParticipantReadyAsync(duelId, userId, true, cancellationToken);

                var allReady = await _duelRepository.AreAllParticipantsReadyAsync(duelId, cancellationToken);

                if (allReady)
                {
                    if (duel.GameServer != null && !string.IsNullOrEmpty(duel.GameServer.ExternalServerId))
                    {
                        await _duelHubService.NotifyServerStarting(duelId);
                        var start = await _dathostService.StartDuelMatchAsync(duel);
                        if (start.Success)
                        {
                            duel.GameServer.Status = ServerStatus.Starting;
                            duel.GameServer.StartedAt = DateTime.UtcNow;
                            await _duelRepository.UpdateStatusAsync(duelId, DuelStatus.Starting, cancellationToken);

                        }
                    }
                    else
                    {
                        var map = duel.Maps.OrderBy(m => m.Order).FirstOrDefault()?.MapName ?? "de_dust2";
                        var steamIds = duel.Participants.Select(p => p.User.SteamId).Where(s => !string.IsNullOrEmpty(s)).ToList();
                        var server = await _dathostService.CreateDuelServerAsync(
                            duel.CreatedBy,
                            duelId,
                            duel.Format,
                            map,
                            steamIds,
                            duel.PrimeOnly,
                            duel.WarmupMinutes,
                            null,
                            duel.CustomMaxRounds);
                        if (server.Success)
                        {
                            var gs = new GameServer
                            {
                                DuelId = duelId,
                                ExternalServerId = server.ServerId,
                                ServerIp = server.ServerIp,
                                ServerPort = server.ServerPort,
                                Password = server.Password,
                                Status = ServerStatus.Starting,
                                StartedAt = DateTime.UtcNow
                            };
                            await _duelRepository.AddGameServerAsync(gs, cancellationToken);
                            await _duelRepository.UpdateStatusAsync(duelId, DuelStatus.Starting, cancellationToken);
                        }
                    }
                }

                var opponent = duel.Participants.FirstOrDefault(p => p.UserId != userId);

                return new ApiResponse<OpponentStatus>
                {
                    Success = true,
                    Data = new OpponentStatus
                    {
                        HasPaid = opponent.HasPaid,
                        IsReady = opponent.IsReady,
                    },
                    Message = allReady ? "All players ready" : "Player ready"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming ready for duel {DuelId}", duelId);
                return new ApiResponse<OpponentStatus>
                {
                    Success = false,
                    Data = new(),
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        private async Task<DathostServerResult> CreateNewServerForDuel(Duel duel, string map, List<string> steamIds, CancellationToken cancellationToken)
        {
            return await _dathostService.CreateDuelServerAsync(
                duel.CreatedBy,
                duel.Id,
                duel.Format,
                map,
                steamIds,
                duel.PrimeOnly,
                duel.WarmupMinutes,
                null,
                duel.CustomMaxRounds);
        }

        private async Task<GameServer> CreateGameServerEntity(DathostServerResult server, Guid duelId, CancellationToken cancellationToken)
        {
            var gs = _mapper.Map<GameServer>(server);
            gs.DuelId = duelId;
            gs.Status = ServerStatus.Stopped;
            gs.StartedAt = null;
            await _duelRepository.AddGameServerAsync(gs, cancellationToken);
            return gs;
        }

        // Helper method moved to service as it's business logic
        private static int GetNextAvailableTeam(Duel duel)
        {
            var teamCounts = duel.Participants.GroupBy(p => p.Team)
                .ToDictionary(g => g.Key, g => g.Count());

            var playersPerTeam = duel.MaxParticipants / 2;

            for (int team = 1; team <= 2; team++)
            {
                if (!teamCounts.ContainsKey(team) || teamCounts[team] < playersPerTeam)
                    return team;
            }

            return 1;
        }
    }
}