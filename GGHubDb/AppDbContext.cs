using GGHubDb.Models;
using Microsoft.EntityFrameworkCore;

namespace GGHubDb
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            try
            {
                Database.EnsureCreated();
            }
            catch (Exception ex)
            {
            }
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Duel> Duels { get; set; }
        public DbSet<DuelParticipant> DuelParticipants { get; set; }
        public DbSet<DuelMap> DuelMaps { get; set; }
        public DbSet<DuelForfeitLog> DuelForfeitLogs { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<GameServer> GameServers { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<TournamentTeam> TournamentTeams { get; set; }
        public DbSet<TournamentPlayer> TournamentPlayers { get; set; }
        public DbSet<TournamentMatch> TournamentMatches { get; set; }
        public DbSet<TournamentMap> TournamentMaps { get; set; }
        public DbSet<TournamentPayment> TournamentPayments { get; set; }
        public DbSet<DuelFormatConfig> DuelFormatConfigs { get; set; }
        public DbSet<ServerRegionConfig> ServerRegionConfigs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Duel relationships
            modelBuilder.Entity<Duel>(entity =>
            {
                entity.HasOne(d => d.Creator)
                    .WithMany()
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Winner)
                    .WithMany()
                    .HasForeignKey(d => d.WinnerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.GameServer)
                    .WithOne(gs => gs.Duel)
                    .HasForeignKey<GameServer>(gs => gs.DuelId);
            });

            modelBuilder.Entity<DuelParticipant>(entity =>
            {
                entity.HasOne(dp => dp.Duel)
                    .WithMany(d => d.Participants)
                    .HasForeignKey(dp => dp.DuelId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(dp => dp.User)
                    .WithMany(u => u.DuelParticipants)
                    .HasForeignKey(dp => dp.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DuelMap>(entity =>
            {
                entity.HasOne(dm => dm.Duel)
                    .WithMany(d => d.Maps)
                    .HasForeignKey(dm => dm.DuelId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GameServer>(entity =>
            {
                entity.HasOne(gs => gs.Duel)
                    .WithOne(d => d.GameServer)
                    .HasForeignKey<GameServer>(gs => gs.DuelId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasOne(t => t.User)
                    .WithMany(u => u.Transactions)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Duel)
                    .WithMany(d => d.Transactions)
                    .HasForeignKey(t => t.DuelId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // DuelForfeitLog relationships
            modelBuilder.Entity<DuelForfeitLog>(entity =>
            {
                entity.HasOne(dfl => dfl.Duel)
                    .WithMany()
                    .HasForeignKey(dfl => dfl.DuelId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(dfl => dfl.User)
                    .WithMany()
                    .HasForeignKey(dfl => dfl.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Complaint relationships
            modelBuilder.Entity<Complaint>(entity =>
            {
                entity.HasOne(c => c.User)
                    .WithMany(u => u.Complaints)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Reviewer)
                    .WithMany()
                    .HasForeignKey(c => c.ReviewedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Duel)
                    .WithMany()
                    .HasForeignKey(c => c.DuelId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Tournament relationships
            modelBuilder.Entity<Tournament>(entity =>
            {
                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Winner)
                    .WithMany()
                    .HasForeignKey(e => e.WinnerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TournamentTeam>(entity =>
            {
                entity.HasOne(e => e.Tournament)
                    .WithMany(t => t.Teams)
                    .HasForeignKey(e => e.TournamentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Captain)
                    .WithMany()
                    .HasForeignKey(e => e.CaptainId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TournamentPlayer>(entity =>
            {
                entity.HasOne(tp => tp.Team)
                    .WithMany(t => t.Players)
                    .HasForeignKey(tp => tp.TeamId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tp => tp.User)
                    .WithMany()
                    .HasForeignKey(tp => tp.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TournamentMatch>(entity =>
            {
                entity.HasOne(tm => tm.Tournament)
                    .WithMany(t => t.Matches)
                    .HasForeignKey(tm => tm.TournamentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tm => tm.Team1)
                    .WithMany()
                    .HasForeignKey(tm => tm.Team1Id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(tm => tm.Team2)
                    .WithMany()
                    .HasForeignKey(tm => tm.Team2Id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(tm => tm.Winner)
                    .WithMany()
                    .HasForeignKey(tm => tm.WinnerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TournamentMap>(entity =>
            {
                entity.HasOne(tm => tm.Tournament)
                    .WithMany(t => t.Maps)
                    .HasForeignKey(tm => tm.TournamentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TournamentPayment>(entity =>
            {
                entity.HasOne(tp => tp.Tournament)
                    .WithMany(t => t.Payments)
                    .HasForeignKey(tp => tp.TournamentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(tp => tp.Team)
                    .WithMany(t => t.Payments)
                    .HasForeignKey(tp => tp.TeamId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(tp => tp.User)
                    .WithMany()
                    .HasForeignKey(tp => tp.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DuelFormatConfig>(entity =>
            {
                entity.HasIndex(e => e.Format).IsUnique();
                entity.Property(e => e.CustomConfig).HasMaxLength(2000);
                entity.Property(e => e.AllowedTickrates).HasMaxLength(50);
            });

            modelBuilder.Entity<ServerRegionConfig>(entity =>
            {
                entity.HasIndex(e => e.RegionCode).IsUnique();
                entity.Property(e => e.RegionCode).IsRequired();
                entity.Property(e => e.PrimaryLocation).IsRequired();
                entity.Property(e => e.FallbackLocations).HasMaxLength(500);
                entity.Property(e => e.CountryCodes).HasMaxLength(1000);
            });
        }
    }
}
