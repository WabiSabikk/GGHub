using GGHubShared.Enums;
using GGHubShared.Models;
using System.ComponentModel.DataAnnotations;

namespace GGHubDb.Models
{
    public class GameServer : BaseEntity
    {
        [Required]
        public Guid DuelId { get; set; }

        public ServerProvider Provider { get; set; } = ServerProvider.Dathost;

        [MaxLength(100)]
        public string? ExternalServerId { get; set; }

        [MaxLength(100)]
        public string? ExternalMatchId { get; set; }

        [MaxLength(255)]
        public string? ServerIp { get; set; }

        public int? ServerPort { get; set; }

        [MaxLength(50)]
        public string? Password { get; set; }

        [MaxLength(50)]
        public string? Rcon { get; set; }

        [MaxLength(255)]
        public string? RawIp { get; set; }

        [MaxLength(50)]
        public string? Location { get; set; }

        public int Slots { get; set; }


        public bool Autostop { get; set; }

        public int AutostopMinutes { get; set; }

        public ServerStatus Status { get; set; } = ServerStatus.Creating;

        [MaxLength(1000)]
        public string? ConfigData { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? StoppedAt { get; set; }

        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        public bool On { get; set; }

        public bool Booting { get; set; }

        public int PlayersOnline { get; set; }

        [MaxLength(100)]
        public string? ServerError { get; set; }

        public bool Confirmed { get; set; }

        public decimal CostPerHour { get; set; }

        public decimal MaxCostPerHour { get; set; }

        public DateTime? MonthResetAt { get; set; }

        public decimal MaxCostPerMonth { get; set; }

        public long ManualSortOrder { get; set; }

        public long DiskUsageBytes { get; set; }

        public bool DeletionProtection { get; set; }

        public bool OngoingMaintenance { get; set; }

        [MaxLength(2000)]
        public string? ServerLogs { get; set; }

        public virtual Duel Duel { get; set; } = null!;
    }
}
