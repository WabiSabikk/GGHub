using GGHubShared.Models;
using System.ComponentModel.DataAnnotations;

namespace GGHubDb.Models
{
    public class DuelParticipant : BaseEntity
    {
        [Required]
        public Guid DuelId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public int Team { get; set; } = 1;

        public bool HasPaid { get; set; } = false;

        public DateTime? PaidAt { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public bool IsReady { get; set; } = false;

        public int? Score { get; set; }

        public int? Kills { get; set; }

        public int? Deaths { get; set; }

        public int? Assists { get; set; }

        /// <summary>
        /// Чи програв гравець через технічну поразку (forfeit)
        /// </summary>
        public bool TechnicalDefeat { get; set; } = false;

        /// <summary>
        /// Причина виходу з дуелі (якщо є)
        /// </summary>
        [MaxLength(500)]
        public string? ForfeitReason { get; set; }

        public virtual Duel Duel { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
