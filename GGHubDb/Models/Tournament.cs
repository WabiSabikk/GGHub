using GGHubShared.Enums;
using GGHubShared.Models;
using System.ComponentModel.DataAnnotations;

namespace GGHubDb.Models
{
    public class Tournament : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int PlayersPerTeam { get; set; }

        public int MaxTeams { get; set; }

        public int CurrentTeams { get; set; } = 0;

        public decimal EntryFee { get; set; }

        public decimal PrizeFund { get; set; }

        public decimal Commission { get; set; }

        public TournamentStatus Status { get; set; } = TournamentStatus.Created;

        [Required]
        public Guid CreatedBy { get; set; }

        public Guid? WinnerId { get; set; }

        public DateTime? PaymentDeadline { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? CompletedAt { get; set; }

        [MaxLength(500)]
        public string? InviteLink { get; set; }

        public int CurrentRound { get; set; } = 0;

        public int TotalRounds { get; set; }

        [MaxLength(1000)]
        public string? Rules { get; set; }

        public virtual User Creator { get; set; } = null!;
        public virtual TournamentTeam? Winner { get; set; }
        public virtual ICollection<TournamentTeam> Teams { get; set; } = new List<TournamentTeam>();
        public virtual ICollection<TournamentMatch> Matches { get; set; } = new List<TournamentMatch>();
        public virtual ICollection<TournamentMap> Maps { get; set; } = new List<TournamentMap>();
        public virtual ICollection<TournamentPayment> Payments { get; set; } = new List<TournamentPayment>();
    }

    public class TournamentTeam : BaseEntity
    {
        [Required]
        public Guid TournamentId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public Guid CaptainId { get; set; }

        public decimal PaidAmount { get; set; } = 0;

        public bool IsPaymentComplete { get; set; } = false;

        public DateTime? PaymentCompletedAt { get; set; }

        [MaxLength(500)]
        public string? JoinToken { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual Tournament Tournament { get; set; } = null!;
        public virtual User Captain { get; set; } = null!;
        public virtual ICollection<TournamentPlayer> Players { get; set; } = new List<TournamentPlayer>();
        public virtual ICollection<TournamentPayment> Payments { get; set; } = new List<TournamentPayment>();
    }

    public class TournamentPlayer : BaseEntity
    {
        [Required]
        public Guid TeamId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public virtual TournamentTeam Team { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }

    public class TournamentMatch : BaseEntity
    {
        [Required]
        public Guid TournamentId { get; set; }

        public int Round { get; set; }

        public int Position { get; set; }

        public Guid? Team1Id { get; set; }

        public Guid? Team2Id { get; set; }

        public Guid? WinnerId { get; set; }

        public TournamentMatchStatus Status { get; set; } = TournamentMatchStatus.Waiting;

        [MaxLength(255)]
        public string? ServerIp { get; set; }

        public int? ServerPort { get; set; }

        [MaxLength(50)]
        public string? ServerPassword { get; set; }

        [MaxLength(100)]
        public string? ExternalServerId { get; set; }

        [MaxLength(100)]
        public string? ExternalMatchId { get; set; }

        public DateTime? ScheduledAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public int? Team1Score { get; set; }

        public int? Team2Score { get; set; }

        [MaxLength(1000)]
        public string? MatchData { get; set; }

        public virtual Tournament Tournament { get; set; } = null!;
        public virtual TournamentTeam? Team1 { get; set; }
        public virtual TournamentTeam? Team2 { get; set; }
        public virtual TournamentTeam? Winner { get; set; }
    }

    public class TournamentMap : BaseEntity
    {
        [Required]
        public Guid TournamentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string MapName { get; set; } = string.Empty;

        public int Order { get; set; }

        public virtual Tournament Tournament { get; set; } = null!;
    }

    public class TournamentPayment : BaseEntity
    {
        [Required]
        public Guid TournamentId { get; set; }

        [Required]
        public Guid TeamId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public decimal Amount { get; set; }

        public PaymentProvider PaymentProvider { get; set; }

        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        [MaxLength(255)]
        public string? ExternalTransactionId { get; set; }

        [MaxLength(500)]
        public string? PaymentUrl { get; set; }

        [MaxLength(2000)]
        public string? PaymentData { get; set; }

        public DateTime? ProcessedAt { get; set; }

        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        public virtual Tournament Tournament { get; set; } = null!;
        public virtual TournamentTeam Team { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}