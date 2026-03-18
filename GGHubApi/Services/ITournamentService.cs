using AutoMapper;
using GGHubDb.Models;
using GGHubDb.Repos;
using GGHubDb.Services;
using GGHubShared.Enums;
using GGHubShared.Models;
using Microsoft.Extensions.Configuration;

namespace GGHubApi.Services
{
    public interface ITournamentService
    {
        Task<ApiResponse<TournamentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<TournamentDto>> GetFullTournamentAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<TournamentDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<TournamentDto>>> GetAvailableTournamentsAsync(CancellationToken cancellationToken = default);
        Task<ApiResponse<List<TournamentDto>>> GetUserTournamentsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<TournamentDto>> CreateTournamentAsync(CreateTournamentRequest request, Guid createdBy, CancellationToken cancellationToken = default);
        Task<ApiResponse<TournamentDto>> UpdateTournamentAsync(Guid tournamentId, UpdateTournamentRequest request, Guid userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<TournamentTeamDto>> JoinTournamentAsync(JoinTournamentRequest request, Guid userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<TournamentTeamDto>> JoinTournamentByTokenAsync(JoinTournamentByTokenRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> LeaveTournamentAsync(Guid tournamentId, Guid userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<TournamentPaymentDto>> CreatePaymentAsync(TournamentPaymentRequest request, Guid userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> StartTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> CancelTournamentAsync(Guid tournamentId, Guid userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<TournamentBracketDto>> GetBracketAsync(Guid tournamentId, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> GenerateInviteLinkAsync(Guid tournamentId, Guid userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ProcessMatchResultAsync(Guid matchId, DathostMatchResult result, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> CreateNextRoundMatchesAsync(Guid tournamentId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ProcessPaymentCompletionAsync(string externalTransactionId, CancellationToken cancellationToken = default);
    }

    public class TournamentService : ITournamentService
    {
        private readonly ITournamentRepository _tournamentRepository;
        private readonly ITournamentTeamRepository _teamRepository;
        private readonly ITournamentMatchRepository _matchRepository;
        private readonly ITournamentPaymentRepository _paymentRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITransactionService _transactionService;
        private readonly IDathostService _dathostService;
        private readonly ICryptomusService _cryptomusService;
        private readonly IMapper _mapper;
        private readonly ILogger<TournamentService> _logger;
        private readonly string _publicBaseUrl;

        public TournamentService(
            ITournamentRepository tournamentRepository,
            ITournamentTeamRepository teamRepository,
            ITournamentMatchRepository matchRepository,
            ITournamentPaymentRepository paymentRepository,
            IUserRepository userRepository,
            ITransactionService transactionService,
            IDathostService dathostService,
            ICryptomusService cryptomusService,
            IMapper mapper,
            ILogger<TournamentService> logger,
            IConfiguration configuration)
        {
            _tournamentRepository = tournamentRepository;
            _teamRepository = teamRepository;
            _matchRepository = matchRepository;
            _paymentRepository = paymentRepository;
            _userRepository = userRepository;
            _transactionService = transactionService;
            _dathostService = dathostService;
            _cryptomusService = cryptomusService;
            _mapper = mapper;
            _logger = logger;
            _publicBaseUrl = configuration["App:PublicBaseUrl"] ?? string.Empty;
        }

        public async Task<ApiResponse<TournamentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var tournament = await _tournamentRepository.GetWithTeamsAsync(id, cancellationToken);
                if (tournament == null)
                {
                    return new ApiResponse<TournamentDto>
                    {
                        Success = false,
                        Code = ErrorCode.NotFound
                    };
                }

                return new ApiResponse<TournamentDto>
                {
                    Success = true,
                    Data = _mapper.Map<TournamentDto>(tournament)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tournament by ID: {TournamentId}", id);
                return new ApiResponse<TournamentDto>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<TournamentDto>> GetFullTournamentAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var tournament = await _tournamentRepository.GetFullTournamentAsync(id, cancellationToken);
                if (tournament == null)
                {
                    return new ApiResponse<TournamentDto>
                    {
                        Success = false,
                        Code = ErrorCode.NotFound
                    };
                }

                return new ApiResponse<TournamentDto>
                {
                    Success = true,
                    Data = _mapper.Map<TournamentDto>(tournament)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting full tournament data: {TournamentId}", id);
                return new ApiResponse<TournamentDto>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<PagedResult<TournamentDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            try
            {
                var (tournaments, totalCount) = await _tournamentRepository.GetPagedAsync(pageNumber, pageSize, cancellationToken: cancellationToken);
                var tournamentDtos = _mapper.Map<List<TournamentDto>>(tournaments);

                return new ApiResponse<PagedResult<TournamentDto>>
                {
                    Success = true,
                    Data = new PagedResult<TournamentDto>
                    {
                        Items = tournamentDtos,
                        TotalCount = totalCount,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged tournaments");
                return new ApiResponse<PagedResult<TournamentDto>>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<List<TournamentDto>>> GetAvailableTournamentsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var tournaments = await _tournamentRepository.GetAvailableTournamentsAsync(cancellationToken);
                var tournamentDtos = _mapper.Map<List<TournamentDto>>(tournaments);

                return new ApiResponse<List<TournamentDto>>
                {
                    Success = true,
                    Data = tournamentDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available tournaments");
                return new ApiResponse<List<TournamentDto>>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<List<TournamentDto>>> GetUserTournamentsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var tournaments = await _tournamentRepository.GetByUserIdAsync(userId, cancellationToken);
                var tournamentDtos = _mapper.Map<List<TournamentDto>>(tournaments);

                return new ApiResponse<List<TournamentDto>>
                {
                    Success = true,
                    Data = tournamentDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user tournaments: {UserId}", userId);
                return new ApiResponse<List<TournamentDto>>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<TournamentDto>> CreateTournamentAsync(CreateTournamentRequest request, Guid createdBy, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(createdBy, cancellationToken);
                if (user == null)
                {
                    return new ApiResponse<TournamentDto>
                    {
                        Success = false,
                        Code = ErrorCode.UserNotFound
                    };
                }

                if (!user.IsPrimeActive)
                {
                    return new ApiResponse<TournamentDto>
                    {
                        Success = false,
                        Code = ErrorCode.Unauthorized
                    };
                }

                if (!IsPowerOfTwo(request.MaxTeams))
                {
                    return new ApiResponse<TournamentDto>
                    {
                        Success = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                var totalEntryFees = request.EntryFee * request.MaxTeams;
                var commission = totalEntryFees * 0.10m;
                var prizeFund = totalEntryFees - commission;

                var tournament = new Tournament
                {
                    Title = request.Title,
                    Description = request.Description,
                    PlayersPerTeam = request.PlayersPerTeam,
                    MaxTeams = request.MaxTeams,
                    EntryFee = request.EntryFee,
                    PrizeFund = prizeFund,
                    Commission = commission,
                    CreatedBy = createdBy,
                    Status = TournamentStatus.Created,
                    StartTime = request.StartTime,
                    Rules = request.Rules,
                    PaymentDeadline = DateTime.UtcNow.AddHours(2)
                };

                var createdTournament = await _tournamentRepository.CreateTournamentWithMapsAsync(tournament, request.Maps, cancellationToken);

                _logger.LogInformation("Created tournament: {TournamentId} by user: {UserId}", createdTournament.Id, createdBy);

                return new ApiResponse<TournamentDto>
                {
                    Success = true,
                    Data = _mapper.Map<TournamentDto>(createdTournament),
                    Message = "Tournament created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tournament for user: {UserId}", createdBy);
                return new ApiResponse<TournamentDto>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<TournamentDto>> UpdateTournamentAsync(Guid tournamentId, UpdateTournamentRequest request, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var tournament = await _tournamentRepository.GetByIdAsync(tournamentId, cancellationToken);
                if (tournament == null)
                {
                    return new ApiResponse<TournamentDto>
                    {
                        Success = false,
                        Code = ErrorCode.NotFound
                    };
                }

                if (tournament.CreatedBy != userId)
                {
                    return new ApiResponse<TournamentDto>
                    {
                        Success = false,
                        Code = ErrorCode.Unauthorized
                    };
                }

                if (!await _tournamentRepository.CanEditTournamentAsync(tournamentId, cancellationToken))
                {
                    return new ApiResponse<TournamentDto>
                    {
                        Success = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                if (request.Title != null) tournament.Title = request.Title;
                if (request.Description != null) tournament.Description = request.Description;
                if (request.PlayersPerTeam.HasValue) tournament.PlayersPerTeam = request.PlayersPerTeam.Value;
                if (request.EntryFee.HasValue)
                {
                    tournament.EntryFee = request.EntryFee.Value;
                    var totalEntryFees = request.EntryFee.Value * tournament.MaxTeams;
                    var commission = totalEntryFees * 0.10m;
                    tournament.PrizeFund = totalEntryFees - commission;
                    tournament.Commission = commission;
                }
                if (request.MaxTeams.HasValue)
                {
                    if (!IsPowerOfTwo(request.MaxTeams.Value))
                    {
                        return new ApiResponse<TournamentDto>
                        {
                            Success = false,
                            Code = ErrorCode.ValidationFailed
                        };
                    }
                    tournament.MaxTeams = request.MaxTeams.Value;
                }
                if (request.StartTime.HasValue) tournament.StartTime = request.StartTime.Value;
                if (request.Rules != null) tournament.Rules = request.Rules;

                var updatedTournament = await _tournamentRepository.UpdateTournamentAsync(tournament, request.Maps, cancellationToken);

                _logger.LogInformation("Updated tournament: {TournamentId} by user: {UserId}", tournamentId, userId);

                return new ApiResponse<TournamentDto>
                {
                    Success = true,
                    Data = _mapper.Map<TournamentDto>(updatedTournament),
                    Message = "Tournament updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tournament: {TournamentId}", tournamentId);
                return new ApiResponse<TournamentDto>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<TournamentTeamDto>> JoinTournamentAsync(JoinTournamentRequest request, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var tournament = await _tournamentRepository.GetWithTeamsAsync(request.TournamentId, cancellationToken);
                if (tournament == null)
                {
                    return new ApiResponse<TournamentTeamDto>
                    {
                        Success = false,
                        Code = ErrorCode.NotFound
                    };
                }

                if (tournament.Status != TournamentStatus.Created && tournament.Status != TournamentStatus.WaitingForTeams)
                {
                    return new ApiResponse<TournamentTeamDto>
                    {
                        Success = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                if (tournament.CurrentTeams >= tournament.MaxTeams)
                {
                    return new ApiResponse<TournamentTeamDto>
                    {
                        Success = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null || !user.IsPrimeActive)
                {
                    return new ApiResponse<TournamentTeamDto>
                    {
                        Success = false,
                        Code = ErrorCode.Unauthorized
                    };
                }

                // Check if user is already in the tournament
                var existingTeam = tournament.Teams.FirstOrDefault(t => t.Players.Any(p => p.UserId == userId));
                if (existingTeam != null)
                {
                    return new ApiResponse<TournamentTeamDto>
                    {
                        Success = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                var team = new TournamentTeam
                {
                    TournamentId = request.TournamentId,
                    Name = request.TeamName,
                    CaptainId = userId
                };

                var createdTeam = await _teamRepository.CreateTeamWithCaptainAsync(team, userId, cancellationToken);

                // Update tournament status if this is the first team
                if (tournament.CurrentTeams == 0)
                {
                    await _tournamentRepository.UpdateStatusAsync(request.TournamentId, TournamentStatus.WaitingForTeams, cancellationToken);
                }

                _logger.LogInformation("User {UserId} joined tournament: {TournamentId} with team: {TeamId}", userId, request.TournamentId, createdTeam.Id);

                return new ApiResponse<TournamentTeamDto>
                {
                    Success = true,
                    Data = _mapper.Map<TournamentTeamDto>(createdTeam),
                    Message = "Successfully joined tournament"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining tournament {TournamentId} for user: {UserId}", request.TournamentId, userId);
                return new ApiResponse<TournamentTeamDto>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<TournamentTeamDto>> JoinTournamentByTokenAsync(JoinTournamentByTokenRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var team = await _teamRepository.GetByJoinTokenAsync(request.JoinToken, cancellationToken);
                if (team == null)
                {
                    return new ApiResponse<TournamentTeamDto>
                    {
                        Success = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                if (team.Players.Count >= team.Tournament.PlayersPerTeam)
                {
                    return new ApiResponse<TournamentTeamDto>
                    {
                        Success = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                if (request.UserId.HasValue)
                {
                    var user = await _userRepository.GetByIdAsync(request.UserId.Value, cancellationToken);
                    if (user == null || !user.IsPrimeActive)
                    {
                        return new ApiResponse<TournamentTeamDto>
                        {
                            Success = false,
                            Code = ErrorCode.Unauthorized
                        };
                    }

                    if (team.Players.Any(p => p.UserId == request.UserId.Value))
                    {
                        return new ApiResponse<TournamentTeamDto>
                        {
                            Success = false,
                            Code = ErrorCode.ValidationFailed
                        };
                    }

                    var updatedTeam = await _teamRepository.AddPlayerToTeamAsync(team.Id, request.UserId.Value, cancellationToken);

                    _logger.LogInformation("User {UserId} joined team: {TeamId} via token", request.UserId.Value, team.Id);

                    return new ApiResponse<TournamentTeamDto>
                    {
                        Success = true,
                        Data = _mapper.Map<TournamentTeamDto>(updatedTeam),
                        Message = "Successfully joined team"
                    };
                }

                return new ApiResponse<TournamentTeamDto>
                {
                    Success = true,
                    Data = _mapper.Map<TournamentTeamDto>(team),
                    Message = "Valid join token"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining team by token: {Token}", request.JoinToken);
                return new ApiResponse<TournamentTeamDto>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> LeaveTournamentAsync(Guid tournamentId, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var tournament = await _tournamentRepository.GetWithTeamsAsync(tournamentId, cancellationToken);
                if (tournament == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.NotFound
                    };
                }

                var userTeam = tournament.Teams.FirstOrDefault(t => t.Players.Any(p => p.UserId == userId));
                if (userTeam == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                if (tournament.Status == TournamentStatus.InProgress || tournament.Status == TournamentStatus.Completed)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                if (userTeam.CaptainId == userId)
                {
                    // If captain leaves, remove entire team
                    await _teamRepository.DeleteAsync(userTeam.Id, cancellationToken);
                }
                else
                {
                    // Remove player from team
                    await _teamRepository.RemovePlayerFromTeamAsync(userTeam.Id, userId, cancellationToken);
                }

                _logger.LogInformation("User {UserId} left tournament: {TournamentId}", userId, tournamentId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Successfully left tournament"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving tournament {TournamentId} for user: {UserId}", tournamentId, userId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<TournamentPaymentDto>> CreatePaymentAsync(TournamentPaymentRequest request, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var tournament = await _tournamentRepository.GetWithTeamsAsync(request.TournamentId, cancellationToken);
                if (tournament == null)
                {
                    return new ApiResponse<TournamentPaymentDto>
                    {
                        Success = false,
                        Code = ErrorCode.NotFound
                    };
                }

                var team = tournament.Teams.FirstOrDefault(t => t.Id == request.TeamId);
                if (team == null)
                {
                    return new ApiResponse<TournamentPaymentDto>
                    {
                        Success = false,
                        Code = ErrorCode.NotFound
                    };
                }

                if (team.IsPaymentComplete)
                {
                    return new ApiResponse<TournamentPaymentDto>
                    {
                        Success = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                var remainingAmount = tournament.EntryFee - team.PaidAmount;
                if (request.Amount > remainingAmount)
                {
                    return new ApiResponse<TournamentPaymentDto>
                    {
                        Success = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                var payment = new TournamentPayment
                {
                    TournamentId = request.TournamentId,
                    TeamId = request.TeamId,
                    UserId = userId,
                    Amount = request.Amount,
                    PaymentProvider = request.PaymentProvider,
                    Status = TransactionStatus.Pending
                };

                if (request.PaymentProvider == PaymentProvider.Cryptomus)
                {
                    var baseUrl = _publicBaseUrl.EndsWith('/') ? _publicBaseUrl : _publicBaseUrl + "/";
                    var cryptomusRequest = new CreateCryptomusPaymentRequest
                    {
                        Amount = request.Amount,
                        OrderId = $"tournament-{request.TournamentId}-{request.TeamId}-{Guid.NewGuid()}",
                        ReturnUrl = $"{baseUrl}tournament/payment/success",
                        CallbackUrl = $"{baseUrl}api/webhook/cryptomus"
                    };

                    var cryptomusResult = await _cryptomusService.CreatePaymentAsync(cryptomusRequest);
                    if (cryptomusResult.Success)
                    {
                        payment.PaymentUrl = cryptomusResult.PaymentUrl;
                        payment.ExternalTransactionId = cryptomusResult.PaymentId;
                    }
                }

                var createdPayment = await _paymentRepository.CreatePaymentAsync(payment, cancellationToken);

                _logger.LogInformation("Created tournament payment: {PaymentId} for team: {TeamId}, amount: {Amount}", createdPayment.Id, request.TeamId, request.Amount);

                return new ApiResponse<TournamentPaymentDto>
                {
                    Success = true,
                    Data = _mapper.Map<TournamentPaymentDto>(createdPayment),
                    Message = "Payment created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tournament payment for team: {TeamId}", request.TeamId);
                return new ApiResponse<TournamentPaymentDto>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> StartTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default)
        {
            try
            {
                var tournament = await _tournamentRepository.GetWithTeamsAsync(tournamentId, cancellationToken);
                if (tournament == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.NotFound
                    };
                }

                if (tournament.Status != TournamentStatus.PaymentCompleted)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                if (tournament.CurrentTeams != tournament.MaxTeams)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                var paidTeams = tournament.Teams.Where(t => t.IsPaymentComplete).ToList();
                if (paidTeams.Count != tournament.MaxTeams)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                // Generate tournament bracket
                var teamIds = paidTeams.Select(t => t.Id).ToList();
                await _matchRepository.GenerateBracketAsync(tournamentId, teamIds, cancellationToken);

                await _tournamentRepository.UpdateStatusAsync(tournamentId, TournamentStatus.InProgress, cancellationToken);

                // Start first round matches
                await CreateServersForRoundAsync(tournamentId, 1, cancellationToken);

                _logger.LogInformation("Started tournament: {TournamentId}", tournamentId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Tournament started successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting tournament: {TournamentId}", tournamentId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> CancelTournamentAsync(Guid tournamentId, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var tournament = await _tournamentRepository.GetByIdAsync(tournamentId, cancellationToken);
                if (tournament == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.NotFound
                    };
                }

                if (tournament.CreatedBy != userId)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.Unauthorized
                    };
                }

                if (tournament.Status == TournamentStatus.InProgress || tournament.Status == TournamentStatus.Completed)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.ValidationFailed
                    };
                }

                await _tournamentRepository.UpdateStatusAsync(tournamentId, TournamentStatus.Cancelled, cancellationToken);

                _logger.LogInformation("Cancelled tournament: {TournamentId} by user: {UserId}", tournamentId, userId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Tournament cancelled successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling tournament: {TournamentId}", tournamentId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<TournamentBracketDto>> GetBracketAsync(Guid tournamentId, CancellationToken cancellationToken = default)
        {
            try
            {
                var tournament = await _tournamentRepository.GetFullTournamentAsync(tournamentId, cancellationToken);
                if (tournament == null)
                {
                    return new ApiResponse<TournamentBracketDto>
                    {
                        Success = false,
                        Code = ErrorCode.NotFound
                    };
                }

                var bracket = new TournamentBracketDto
                {
                    TournamentId = tournamentId,
                    TotalRounds = tournament.TotalRounds,
                    CurrentRound = tournament.CurrentRound,
                    Rounds = new List<TournamentRoundDto>()
                };

                for (int round = 1; round <= tournament.TotalRounds; round++)
                {
                    var roundMatches = tournament.Matches.Where(m => m.Round == round).ToList();
                    var roundDto = new TournamentRoundDto
                    {
                        Round = round,
                        RoundName = GetRoundName(round, tournament.TotalRounds),
                        Matches = _mapper.Map<List<TournamentMatchDto>>(roundMatches)
                    };

                    bracket.Rounds.Add(roundDto);
                }

                return new ApiResponse<TournamentBracketDto>
                {
                    Success = true,
                    Data = bracket
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tournament bracket: {TournamentId}", tournamentId);
                return new ApiResponse<TournamentBracketDto>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<string>> GenerateInviteLinkAsync(Guid tournamentId, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var tournament = await _tournamentRepository.GetByIdAsync(tournamentId, cancellationToken);
                if (tournament == null)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Code = ErrorCode.NotFound
                    };
                }

                if (tournament.CreatedBy != userId)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Code = ErrorCode.Unauthorized
                    };
                }

                var inviteLink = await _tournamentRepository.GenerateInviteLinkAsync(tournamentId, cancellationToken);
                if (inviteLink == null)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Code = ErrorCode.ServerError
                    };
                }

                return new ApiResponse<string>
                {
                    Success = true,
                    Data = inviteLink,
                    Message = "Invite link generated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invite link for tournament: {TournamentId}", tournamentId);
                return new ApiResponse<string>
                {
                    Success = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> ProcessMatchResultAsync(Guid matchId, DathostMatchResult result, CancellationToken cancellationToken = default)
        {
            try
            {
                var match = await _matchRepository.GetWithTeamsAsync(matchId, cancellationToken);
                if (match == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.NotFound
                    };
                }

                var winnerId = result.WinnerTeam == "team_a" ? match.Team1Id : match.Team2Id;
                if (winnerId.HasValue)
                {
                    await _matchRepository.UpdateMatchResultAsync(matchId, winnerId.Value, result.Team1Score, result.Team2Score, cancellationToken);
                }

                if (!string.IsNullOrEmpty(match.ExternalServerId))
                {
                    await _dathostService.StopServerAsync(match.ExternalServerId);
                }

                // Check if round is complete
                var roundMatches = await _matchRepository.GetByRoundAsync(match.TournamentId, match.Round, cancellationToken);
                var completedMatches = roundMatches.Where(m => m.Status == TournamentMatchStatus.Completed).ToList();

                if (completedMatches.Count == roundMatches.Count())
                {
                    await CreateNextRoundMatchesAsync(match.TournamentId, cancellationToken);
                }

                _logger.LogInformation("Processed match result: {MatchId}, Winner: {WinnerId}", matchId, winnerId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Match result processed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing match result: {MatchId}", matchId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> CreateNextRoundMatchesAsync(Guid tournamentId, CancellationToken cancellationToken = default)
        {
            try
            {
                var tournament = await _tournamentRepository.GetFullTournamentAsync(tournamentId, cancellationToken);
                if (tournament == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.NotFound
                    };
                }

                var currentRound = tournament.CurrentRound;
                var nextRound = currentRound + 1;

                if (nextRound > tournament.TotalRounds)
                {
                    // Tournament is complete
                    var finalMatch = tournament.Matches.FirstOrDefault(m => m.Round == tournament.TotalRounds);
                    if (finalMatch?.WinnerId.HasValue == true)
                    {
                        await _tournamentRepository.UpdateStatusAsync(tournamentId, TournamentStatus.Completed, cancellationToken);

                        // Award prize to winner
                        await _transactionService.CreatePrizeAsync(finalMatch.Winner!.CaptainId, tournamentId, tournament.PrizeFund, cancellationToken);
                    }

                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Tournament completed"
                    };
                }

                // Update teams for next round matches
                var currentRoundMatches = tournament.Matches.Where(m => m.Round == currentRound).OrderBy(m => m.Position).ToList();
                var nextRoundMatches = tournament.Matches.Where(m => m.Round == nextRound).OrderBy(m => m.Position).ToList();

                for (int i = 0; i < nextRoundMatches.Count; i++)
                {
                    var nextMatch = nextRoundMatches[i];
                    var match1 = currentRoundMatches[i * 2];
                    var match2 = currentRoundMatches[i * 2 + 1];

                    nextMatch.Team1Id = match1.WinnerId;
                    nextMatch.Team2Id = match2.WinnerId;
                }

                // Create servers for next round
                await CreateServersForRoundAsync(tournamentId, nextRound, cancellationToken);

                _logger.LogInformation("Created next round matches for tournament: {TournamentId}, Round: {Round}", tournamentId, nextRound);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Next round matches created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating next round matches for tournament: {TournamentId}", tournamentId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> ProcessPaymentCompletionAsync(string externalTransactionId, CancellationToken cancellationToken = default)
        {
            try
            {
                var payment = await _paymentRepository.GetByExternalIdAsync(externalTransactionId, cancellationToken);
                if (payment == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.NotFound
                    };
                }

                await _paymentRepository.UpdatePaymentStatusAsync(payment.Id, TransactionStatus.Completed, cancellationToken: cancellationToken);

                // Update team payment status
                var teamPayments = await _paymentRepository.GetByTeamIdAsync(payment.TeamId, cancellationToken);
                var totalPaid = teamPayments.Where(p => p.Status == TransactionStatus.Completed).Sum(p => p.Amount);
                var isComplete = totalPaid >= payment.Tournament.EntryFee;

                await _teamRepository.UpdatePaymentStatusAsync(payment.TeamId, totalPaid, isComplete, cancellationToken);

                if (isComplete)
                {
                    // Check if all teams have paid
                    var tournament = await _tournamentRepository.GetWithTeamsAsync(payment.TournamentId, cancellationToken);
                    var paidTeams = tournament!.Teams.Where(t => t.IsPaymentComplete).Count();

                    if (paidTeams == tournament.MaxTeams)
                    {
                        await _tournamentRepository.UpdateStatusAsync(payment.TournamentId, TournamentStatus.PaymentCompleted, cancellationToken);
                    }
                }

                _logger.LogInformation("Processed payment completion: {PaymentId}", payment.Id);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Payment processed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment completion: {ExternalId}", externalTransactionId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Code = ErrorCode.ServerError,
                    Errors = { ex.Message }
                };
            }
        }

        private async Task CreateServersForRoundAsync(Guid tournamentId, int round, CancellationToken cancellationToken)
        {
            var matches = await _matchRepository.GetByRoundAsync(tournamentId, round, cancellationToken);
            var tournament = await _tournamentRepository.GetFullTournamentAsync(tournamentId, cancellationToken);

            foreach (var match in matches.Where(m => m.Team1Id.HasValue && m.Team2Id.HasValue))
            {
                await _matchRepository.UpdateMatchStatusAsync(match.Id, TournamentMatchStatus.CreatingServer, cancellationToken);

                var team1Players = match.Team1!.Players.Select(p => p.User.SteamId).Where(s => !string.IsNullOrEmpty(s)).ToList();
                var team2Players = match.Team2!.Players.Select(p => p.User.SteamId).Where(s => !string.IsNullOrEmpty(s)).ToList();
                var allPlayers = team1Players.Concat(team2Players).ToList();

                var mapName = tournament!.Maps.FirstOrDefault()?.MapName ?? "de_dust2";

                //var serverResult = await _dathostService.CreateTournamentServerAsync(tournamentId, match.Id, mapName, allPlayers);
                //if (serverResult.Success)
                //{
                //    await _matchRepository.UpdateServerInfoAsync(match.Id, serverResult.ServerIp!, serverResult.ServerPort!.Value,
                //        serverResult.Password!, serverResult.ServerId, cancellationToken);
                //    await _matchRepository.UpdateMatchStatusAsync(match.Id, TournamentMatchStatus.Ready, cancellationToken);
                //}
                //else
                //{
                //    await _matchRepository.UpdateMatchStatusAsync(match.Id, TournamentMatchStatus.Cancelled, cancellationToken);
                //}
            }
        }

        private bool IsPowerOfTwo(int value)
        {
            return value > 0 && (value & (value - 1)) == 0;
        }

        private string GetRoundName(int round, int totalRounds)
        {
            if (round == totalRounds) return "Final";
            if (round == totalRounds - 1) return "Semi-Final";
            if (round == totalRounds - 2) return "Quarter-Final";
            return $"Round {round}";
        }
    }
}