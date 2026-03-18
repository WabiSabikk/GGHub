using GGHubShared.Enums;
using GGHubShared.Models;
using System.ComponentModel.DataAnnotations;

namespace GGHubDb.Models
{

    public class DuelForfeitLog : BaseEntity
    {
        [Required]
        public Guid DuelId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public ForfeitReason Reason { get; set; }

    
        public int? MeasuredPing { get; set; }

      
        public float? PacketLossIn { get; set; }

  
        public float? PacketLossOut { get; set; }

   
        public bool AutoRefunded { get; set; }

      
        public bool AdminReviewed { get; set; }


        [MaxLength(1000)]
        public string? AdminNotes { get; set; }

    
        [MaxLength(2000)]
        public string? ServerLogs { get; set; }

        public virtual Duel Duel { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
