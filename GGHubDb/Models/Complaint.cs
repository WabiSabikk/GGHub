using GGHubShared.Enums;
using GGHubShared.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GGHubDb.Models
{
    public class Complaint : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid DuelId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;

        [MaxLength(2000)]
        public string? Evidence { get; set; }

        public Guid? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [MaxLength(2000)]
        public string? ReviewNotes { get; set; }

        [MaxLength(1000)]
        public string? Resolution { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Duel Duel { get; set; } = null!;

        public virtual User? Reviewer { get; set; }
    }
}
