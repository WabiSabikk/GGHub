using System.ComponentModel.DataAnnotations;
using GGHubBot.Enums;

namespace GGHubBot.Models
{
    public class DuelServerNotification
    {
        [Key]
        public int Id { get; set; }
        public Guid DuelId { get; set; }
        public long TelegramId { get; set; }
        public DuelServerNotificationType Type { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
