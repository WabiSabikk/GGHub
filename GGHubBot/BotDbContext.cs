using GGHubBot.Models;
using Microsoft.EntityFrameworkCore;

namespace GGHubBot
{
    public class BotDbContext : DbContext
    {
        public BotDbContext(DbContextOptions<BotDbContext> options) : base(options) { }

        public DbSet<UserState> UserStates { get; set; }
        public DbSet<DuelServerNotification> DuelServerNotifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserState>(entity =>
            {
                entity.HasKey(e => e.TelegramId);
                entity.Property(e => e.Language).HasMaxLength(5);
                entity.Property(e => e.StateData).HasMaxLength(4000);
                entity.Property(e => e.SteamId).HasMaxLength(100);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.SteamId);
                entity.HasIndex(e => e.LastActivity);
            });

            modelBuilder.Entity<DuelServerNotification>(entity =>
            {
                entity.HasIndex(e => new { e.DuelId, e.TelegramId, e.Type }).IsUnique();
            });
        }
    }
}
