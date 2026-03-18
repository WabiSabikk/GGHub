using GGHubShared.Enums;
using GGHubShared.Models;
using System.ComponentModel.DataAnnotations;

namespace GGHubDb.Models
{
    public class User : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? PasswordHash { get; set; }

        [MaxLength(100)]
        public string? SteamId { get; set; }

        [MaxLength(100)]
        public string? DiscordId { get; set; }

        [MaxLength(50)]
        public string? TelegramUsername { get; set; }

        public long? TelegramChatId { get; set; }

        public UserRole Role { get; set; } = UserRole.User;

        public bool IsPrimeActive { get; set; } = false;

        public DateTime? PrimeExpiresAt { get; set; }

        public decimal Balance { get; set; } = 0m;

        public int Wins { get; set; } = 0;

        public int Losses { get; set; } = 0;

        public int Rating { get; set; } = 1000;

        public bool IsEmailVerified { get; set; } = false;

        public string? AvatarUrl { get; set; }
        public string? Country { get; set; }
        public string? EmailVerificationToken { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public virtual ICollection<DuelParticipant> DuelParticipants { get; set; } = new List<DuelParticipant>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    }
}
