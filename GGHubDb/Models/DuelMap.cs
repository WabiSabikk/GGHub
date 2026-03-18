using GGHubShared.Models;
using System.ComponentModel.DataAnnotations;

namespace GGHubDb.Models
{
    public class DuelMap : BaseEntity
    {
        [Required]
        public Guid DuelId { get; set; }

        [Required]
        [MaxLength(50)]
        public string MapName { get; set; } = string.Empty;

        public int Order { get; set; }

        public bool IsPlayed { get; set; } = false;

        public int? Team1Score { get; set; }

        public int? Team2Score { get; set; }

        public DateTime? PlayedAt { get; set; }

        [MaxLength(1000)]
        public string? MatchData { get; set; }

        public virtual Duel Duel { get; set; } = null!;
    }
}
