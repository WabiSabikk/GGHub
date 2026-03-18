using GGHubShared.Enums;
using System.ComponentModel.DataAnnotations;

namespace GGHubShared.Models
{
    public class CreateDuelRequest
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        public DuelFormat Format { get; set; }

        public RoundFormat RoundFormat { get; set; }

        [Range(1, 1000)]
        public decimal EntryFee { get; set; }

        public bool PrimeOnly { get; set; }

        [Range(3, 5)]
        public int WarmupMinutes { get; set; } = 5;

        [Required]
        [MinLength(1)]
        public List<string> Maps { get; set; } = new();

        // Extended configuration options
        public string? PreferredRegion { get; set; }
        public int? CustomTickrate { get; set; }
        public int? CustomMaxRounds { get; set; }
        public bool? CustomOvertimeEnabled { get; set; }
    }

    public class JoinDuelRequest
    {
        [Required]
        public Guid DuelId { get; set; }

        public int? Team { get; set; }
    }

    public class ProcessPaymentRequest
    {
        [Required]
        public Guid DuelId { get; set; }

        public PaymentProvider PaymentProvider { get; set; }

        [Range(0.01, 10000)]
        public decimal Amount { get; set; }
    }

    public class CreateComplaintRequest
    {
        [Required]
        public Guid DuelId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Evidence { get; set; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? SteamId { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsPrimeActive { get; set; }
        public decimal Balance { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DuelDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DuelFormat Format { get; set; }
        public RoundFormat RoundFormat { get; set; }
        public decimal EntryFee { get; set; }
        public decimal PrizeFund { get; set; }
        public bool PrimeOnly { get; set; }
        public int WarmupMinutes { get; set; }
        public int? CustomMaxRounds { get; set; }
        public int? CustomTickrate { get; set; }
        public bool? CustomOvertimeEnabled { get; set; }
        public string? PreferredRegion { get; set; }
        public string? ServerConfig { get; set; }
        public DuelStatus Status { get; set; }
        public int MaxParticipants { get; set; }
        public int CurrentParticipants { get; set; }
        public UserDto Creator { get; set; } = null!;
        public UserDto? Winner { get; set; }
        public string? InviteLink { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<DuelParticipantDto> Participants { get; set; } = new();
        public List<string> Maps { get; set; } = new();
        public GameServerDto? GameServer { get; set; }
    }

    public class GameServerDto
    {
        public string ServerIp { get; set; } = string.Empty;
        public int ServerPort { get; set; }
        public string? Password { get; set; }

        public string? SteamUrl => $"steam://run/730//+connect {ServerIp}:{ServerPort}";
    }

    public class DuelParticipantDto
    {
        public Guid Id { get; set; }
        public UserDto User { get; set; } = null!;
        public int Team { get; set; }
        public bool HasPaid { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsReady { get; set; }
        public int? Score { get; set; }
        public int? Kills { get; set; }
        public int? Deaths { get; set; }
        public int? Assists { get; set; }
    }

    public class TransactionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? DuelId { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public TransactionStatus Status { get; set; }
        public PaymentProvider? PaymentProvider { get; set; }
        public string? PaymentUrl { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public ErrorCode Code { get; set; } = ErrorCode.None;
        public List<string> Errors { get; set; } = new();
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }

    public class CreateTournamentRequest
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(1, 5)]
        public int PlayersPerTeam { get; set; }

        [Range(2, 32)]
        public int MaxTeams { get; set; }

        [Range(1, 10000)]
        public decimal EntryFee { get; set; }

        [Required]
        [MinLength(1)]
        public List<string> Maps { get; set; } = new();

        public DateTime? StartTime { get; set; }

        [MaxLength(1000)]
        public string? Rules { get; set; }
    }

    public class UpdateTournamentRequest
    {
        [MaxLength(100)]
        public string? Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(1, 5)]
        public int? PlayersPerTeam { get; set; }

        [Range(2, 32)]
        public int? MaxTeams { get; set; }

        [Range(1, 10000)]
        public decimal? EntryFee { get; set; }

        public List<string>? Maps { get; set; }

        public DateTime? StartTime { get; set; }

        [MaxLength(1000)]
        public string? Rules { get; set; }
    }

    public class JoinTournamentRequest
    {
        [Required]
        public Guid TournamentId { get; set; }

        [Required]
        [MaxLength(100)]
        public string TeamName { get; set; } = string.Empty;

        public List<Guid> PlayerIds { get; set; } = new();
    }

    public class JoinTournamentByTokenRequest
    {
        [Required]
        public string JoinToken { get; set; } = string.Empty;

        public Guid? UserId { get; set; }
    }

    public class TournamentPaymentRequest
    {
        [Required]
        public Guid TournamentId { get; set; }

        [Required]
        public Guid TeamId { get; set; }

        [Range(0.01, 10000)]
        public decimal Amount { get; set; }

        public PaymentProvider PaymentProvider { get; set; } = PaymentProvider.Cryptomus;
    }

    public class TournamentDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int PlayersPerTeam { get; set; }
        public int MaxTeams { get; set; }
        public int CurrentTeams { get; set; }
        public decimal EntryFee { get; set; }
        public decimal PrizeFund { get; set; }
        public TournamentStatus Status { get; set; }
        public UserDto Creator { get; set; } = null!;
        public TournamentTeamDto? Winner { get; set; }
        public DateTime? PaymentDeadline { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? InviteLink { get; set; }
        public int CurrentRound { get; set; }
        public int TotalRounds { get; set; }
        public string? Rules { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<TournamentTeamDto> Teams { get; set; } = new();
        public List<TournamentMatchDto> Matches { get; set; } = new();
        public List<string> Maps { get; set; } = new();
    }

    public class TournamentTeamDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public UserDto Captain { get; set; } = null!;
        public decimal PaidAmount { get; set; }
        public bool IsPaymentComplete { get; set; }
        public DateTime? PaymentCompletedAt { get; set; }
        public string? JoinToken { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<TournamentPlayerDto> Players { get; set; } = new();
    }

    public class TournamentPlayerDto
    {
        public Guid Id { get; set; }
        public UserDto User { get; set; } = null!;
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class TournamentMatchDto
    {
        public Guid Id { get; set; }
        public int Round { get; set; }
        public int Position { get; set; }
        public TournamentTeamDto? Team1 { get; set; }
        public TournamentTeamDto? Team2 { get; set; }
        public TournamentTeamDto? Winner { get; set; }
        public TournamentMatchStatus Status { get; set; }
        public string? ServerIp { get; set; }
        public int? ServerPort { get; set; }
        public string? ServerPassword { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? Team1Score { get; set; }
        public int? Team2Score { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TournamentBracketDto
    {
        public Guid TournamentId { get; set; }
        public int TotalRounds { get; set; }
        public int CurrentRound { get; set; }
        public List<TournamentRoundDto> Rounds { get; set; } = new();
    }

    public class TournamentRoundDto
    {
        public int Round { get; set; }
        public string RoundName { get; set; } = string.Empty;
        public List<TournamentMatchDto> Matches { get; set; } = new();
    }

    public class TournamentPaymentDto
    {
        public Guid Id { get; set; }
        public TournamentTeamDto Team { get; set; } = null!;
        public UserDto User { get; set; } = null!;
        public decimal Amount { get; set; }
        public PaymentProvider PaymentProvider { get; set; }
        public TransactionStatus Status { get; set; }
        public string? PaymentUrl { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
    }

    public class JoinDuelByInviteLinkRequest
    {
        [Required]
        public string InviteLink { get; set; } = string.Empty;
    }

    public class CompleteDuelRequest
    {
        [Required]
        public Guid WinnerId { get; set; }
    }

    public class UpdateDuelStatusRequest
    {
        [Required]
        public DuelStatus Status { get; set; }
    }

    public class GlobalMatchMetricsDto
    {
        public int TotalMatches { get; set; }
        public decimal AverageDeposit { get; set; }
    }

    public class UserMatchMetricsDto
    {
        public int MatchesPlayed { get; set; }
        public decimal AverageDeposit { get; set; }
    }

    public class OpponentStatus
    {
        public bool HasPaid { get; set; }
        public bool IsReady { get; set; }
    }

    public class ForfeitDuelRequest
    {
        [Required]
        public ForfeitReason Reason { get; set; }
    }

    public class DuelForfeitResult
    {
        public bool Forfeited { get; set; }
        public bool RefundIssued { get; set; }
        public Guid? WinnerId { get; set; }
        public int? MeasuredPing { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
