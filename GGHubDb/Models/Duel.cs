using GGHubShared.Enums;
using GGHubShared.Models;
using System.ComponentModel.DataAnnotations;

namespace GGHubDb.Models
{
    public class Duel : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        public DuelFormat Format { get; set; }

        public RoundFormat RoundFormat { get; set; }

        public decimal EntryFee { get; set; }

        public decimal PrizeFund { get; set; }

        public decimal Commission { get; set; }

        public bool PrimeOnly { get; set; }

        public int WarmupMinutes { get; set; } = 5;

        public int? CustomMaxRounds { get; set; }

        public int? CustomTickrate { get; set; }

        public bool? CustomOvertimeEnabled { get; set; }

        [MaxLength(50)]
        public string? PreferredRegion { get; set; }

        [MaxLength(1000)]
        public string? ServerConfig { get; set; }

        public DuelStatus Status { get; set; } = DuelStatus.Created;

        public int MaxParticipants { get; set; }

        public int CurrentParticipants { get; set; } = 0;

        [Required]
        public Guid CreatedBy { get; set; }

        public Guid? WinnerId { get; set; }

        [MaxLength(500)]
        public string? InviteLink { get; set; }

        [MaxLength(255)]
        public string? ServerIp { get; set; }

        [MaxLength(50)]
        public string? ServerPassword { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public virtual User Creator { get; set; } = null!;
        public virtual User? Winner { get; set; }
        public virtual ICollection<DuelParticipant> Participants { get; set; } = new List<DuelParticipant>();
        public virtual ICollection<DuelMap> Maps { get; set; } = new List<DuelMap>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public virtual GameServer? GameServer { get; set; }
    }
}
