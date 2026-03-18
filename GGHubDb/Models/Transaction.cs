using GGHubShared.Enums;
using GGHubShared.Models;
using System.ComponentModel.DataAnnotations;

namespace GGHubDb.Models
{
    public class Transaction : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }

        public Guid? DuelId { get; set; }

        public TransactionType Type { get; set; }

        public decimal Amount { get; set; }

        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        public PaymentProvider? PaymentProvider { get; set; }

        [MaxLength(255)]
        public string? ExternalTransactionId { get; set; }

        [MaxLength(500)]
        public string? PaymentUrl { get; set; }

        public DateTime? ExpiresAt { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(2000)]
        public string? PaymentData { get; set; }

        public DateTime? ProcessedAt { get; set; }

        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Duel? Duel { get; set; }
    }
}
