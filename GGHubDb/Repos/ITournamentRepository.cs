using GGHubDb.Models;
using GGHubShared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GGHubDb.Repos
{
    public interface ITournamentRepository : IBaseRepository<Tournament>
    {
        Task<Tournament?> GetWithTeamsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Tournament?> GetFullTournamentAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Tournament>> GetByStatusAsync(TournamentStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Tournament>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Tournament>> GetAvailableTournamentsAsync(CancellationToken cancellationToken = default);
        Task<Tournament> CreateTournamentWithMapsAsync(Tournament tournament, List<string> maps, CancellationToken cancellationToken = default);
        Task UpdateStatusAsync(Guid tournamentId, TournamentStatus status, CancellationToken cancellationToken = default);
        Task<string?> GenerateInviteLinkAsync(Guid tournamentId, CancellationToken cancellationToken = default);
        Task<Tournament?> GetByInviteLinkAsync(string inviteLink, CancellationToken cancellationToken = default);
        Task<bool> CanEditTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default);
        Task<Tournament> UpdateTournamentAsync(Tournament tournament, List<string>? maps = null, CancellationToken cancellationToken = default);
    }

    public interface ITournamentTeamRepository : IBaseRepository<TournamentTeam>
    {
        Task<TournamentTeam?> GetWithPlayersAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TournamentTeam>> GetByTournamentIdAsync(Guid tournamentId, CancellationToken cancellationToken = default);
        Task<TournamentTeam?> GetByJoinTokenAsync(string joinToken, CancellationToken cancellationToken = default);
        Task<TournamentTeam> CreateTeamWithCaptainAsync(TournamentTeam team, Guid captainId, CancellationToken cancellationToken = default);
        Task<TournamentTeam> AddPlayerToTeamAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default);
        Task RemovePlayerFromTeamAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default);
        Task UpdatePaymentStatusAsync(Guid teamId, decimal amount, bool isComplete, CancellationToken cancellationToken = default);
        Task<string> GenerateJoinTokenAsync(Guid teamId, CancellationToken cancellationToken = default);
    }

    public interface ITournamentMatchRepository : IBaseRepository<TournamentMatch>
    {
        Task<TournamentMatch?> GetWithTeamsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TournamentMatch>> GetByTournamentIdAsync(Guid tournamentId, CancellationToken cancellationToken = default);
        Task<IEnumerable<TournamentMatch>> GetByRoundAsync(Guid tournamentId, int round, CancellationToken cancellationToken = default);
        Task<IEnumerable<TournamentMatch>> GetByStatusAsync(TournamentMatchStatus status, CancellationToken cancellationToken = default);
        Task<TournamentMatch> CreateMatchAsync(TournamentMatch match, CancellationToken cancellationToken = default);
        Task UpdateMatchStatusAsync(Guid matchId, TournamentMatchStatus status, CancellationToken cancellationToken = default);
        Task UpdateMatchResultAsync(Guid matchId, Guid winnerId, int team1Score, int team2Score, CancellationToken cancellationToken = default);
        Task UpdateServerInfoAsync(Guid matchId, string serverIp, int serverPort, string password, string? serverId = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<TournamentMatch>> GenerateBracketAsync(Guid tournamentId, List<Guid> teamIds, CancellationToken cancellationToken = default);
        Task<TournamentMatch?> GetByExternalMatchIdAsync(string externalMatchId, CancellationToken cancellationToken = default);

        Task CompleteMatchAsync(Guid matchId, Guid winnerId, int team1Score, int team2Score, CancellationToken cancellationToken = default);
    }

    public interface ITournamentPaymentRepository : IBaseRepository<TournamentPayment>
    {
        Task<IEnumerable<TournamentPayment>> GetByTournamentIdAsync(Guid tournamentId, CancellationToken cancellationToken = default);
        Task<IEnumerable<TournamentPayment>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
        Task<TournamentPayment?> GetByExternalIdAsync(string externalTransactionId, CancellationToken cancellationToken = default);
        Task<TournamentPayment> CreatePaymentAsync(TournamentPayment payment, CancellationToken cancellationToken = default);
        Task UpdatePaymentStatusAsync(Guid paymentId, TransactionStatus status, string? externalId = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<TournamentPayment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default);
    }

    public class TournamentRepository : BaseRepository<Tournament>, ITournamentRepository
    {
        private readonly string _publicBaseUrl;

        public TournamentRepository(
            AppDbContext context,
            ILogger<TournamentRepository> logger,
            IConfiguration configuration)
            : base(context, logger)
        {
            _publicBaseUrl = configuration["App:PublicBaseUrl"] ?? string.Empty;
        }

        public async Task<Tournament?> GetWithTeamsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(t => t.Teams)
                    .ThenInclude(team => team.Players)
                        .ThenInclude(p => p.User)
                .Include(t => t.Teams)
                    .ThenInclude(team => team.Captain)
                .Include(t => t.Creator)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
        }

        public async Task<Tournament?> GetFullTournamentAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(t => t.Teams)
                    .ThenInclude(team => team.Players)
                        .ThenInclude(p => p.User)
                .Include(t => t.Teams)
                    .ThenInclude(team => team.Captain)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.Team1)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.Team2)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.Winner)
                .Include(t => t.Maps)
                .Include(t => t.Creator)
                .Include(t => t.Winner)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
        }

        public async Task<IEnumerable<Tournament>> GetByStatusAsync(TournamentStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.Status == status && !t.IsDeleted)
                .Include(t => t.Creator)
                .Include(t => t.Teams)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Tournament>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => !t.IsDeleted && (t.CreatedBy == userId || t.Teams.Any(team => team.Players.Any(p => p.UserId == userId))))
                .Include(t => t.Creator)
                .Include(t => t.Teams)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Tournament>> GetAvailableTournamentsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => !t.IsDeleted && t.Status == TournamentStatus.WaitingForTeams && t.CurrentTeams < t.MaxTeams)
                .Include(t => t.Creator)
                .Include(t => t.Teams)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Tournament> CreateTournamentWithMapsAsync(Tournament tournament, List<string> maps, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                tournament.TotalRounds = (int)Math.Ceiling(Math.Log2(tournament.MaxTeams));
                tournament.CreatedAt = DateTime.UtcNow;

                await _dbSet.AddAsync(tournament, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                foreach (var (mapName, index) in maps.Select((map, i) => (map, i)))
                {
                    var tournamentMap = new TournamentMap
                    {
                        TournamentId = tournament.Id,
                        MapName = mapName,
                        Order = index + 1,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.TournamentMaps.AddAsync(tournamentMap, cancellationToken);
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return await GetFullTournamentAsync(tournament.Id, cancellationToken) ?? tournament;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task UpdateStatusAsync(Guid tournamentId, TournamentStatus status, CancellationToken cancellationToken = default)
        {
            var tournament = await GetByIdAsync(tournamentId, cancellationToken);
            if (tournament == null) throw new InvalidOperationException($"Tournament {tournamentId} not found");

            tournament.Status = status;
            tournament.UpdatedAt = DateTime.UtcNow;

            if (status == TournamentStatus.InProgress && !tournament.StartTime.HasValue)
                tournament.StartTime = DateTime.UtcNow;
            else if (status == TournamentStatus.Completed && !tournament.CompletedAt.HasValue)
                tournament.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<string?> GenerateInviteLinkAsync(Guid tournamentId, CancellationToken cancellationToken = default)
        {
            var tournament = await GetByIdAsync(tournamentId, cancellationToken);
            if (tournament == null) return null;

            var inviteCode = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("=", "").Replace("+", "-").Replace("/", "_")[..8];

            var baseUrl = _publicBaseUrl.EndsWith('/') ? _publicBaseUrl : _publicBaseUrl + "/";
            tournament.InviteLink = $"{baseUrl}tournament/join/{inviteCode}";
            tournament.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return tournament.InviteLink;
        }

        public async Task<Tournament?> GetByInviteLinkAsync(string inviteLink, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(t => t.InviteLink == inviteLink && !t.IsDeleted, cancellationToken);
        }

        public async Task<bool> CanEditTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default)
        {
            var tournament = await GetByIdAsync(tournamentId, cancellationToken);
            return tournament != null &&
                   (tournament.Status == TournamentStatus.Created || tournament.Status == TournamentStatus.WaitingForTeams);
        }

        public async Task<Tournament> UpdateTournamentAsync(Tournament tournament, List<string>? maps = null, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                tournament.UpdatedAt = DateTime.UtcNow;
                tournament.TotalRounds = (int)Math.Ceiling(Math.Log2(tournament.MaxTeams));

                _dbSet.Update(tournament);

                if (maps != null)
                {
                    var existingMaps = await _context.TournamentMaps
                        .Where(m => m.TournamentId == tournament.Id)
                        .ToListAsync(cancellationToken);

                    _context.TournamentMaps.RemoveRange(existingMaps);

                    foreach (var (mapName, index) in maps.Select((map, i) => (map, i)))
                    {
                        var tournamentMap = new TournamentMap
                        {
                            TournamentId = tournament.Id,
                            MapName = mapName,
                            Order = index + 1,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _context.TournamentMaps.AddAsync(tournamentMap, cancellationToken);
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return await GetFullTournamentAsync(tournament.Id, cancellationToken) ?? tournament;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    public class TournamentTeamRepository : BaseRepository<TournamentTeam>, ITournamentTeamRepository
    {
        public TournamentTeamRepository(AppDbContext context, ILogger<TournamentTeamRepository> logger) : base(context, logger) { }

        public async Task<TournamentTeam?> GetWithPlayersAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(t => t.Players)
                    .ThenInclude(p => p.User)
                .Include(t => t.Captain)
                .Include(t => t.Tournament)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
        }

        public async Task<IEnumerable<TournamentTeam>> GetByTournamentIdAsync(Guid tournamentId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.TournamentId == tournamentId && !t.IsDeleted)
                .Include(t => t.Players)
                    .ThenInclude(p => p.User)
                .Include(t => t.Captain)
                .ToListAsync(cancellationToken);
        }

        public async Task<TournamentTeam?> GetByJoinTokenAsync(string joinToken, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(t => t.Players)
                    .ThenInclude(p => p.User)
                .Include(t => t.Captain)
                .Include(t => t.Tournament)
                .FirstOrDefaultAsync(t => t.JoinToken == joinToken && !t.IsDeleted, cancellationToken);
        }

        public async Task<TournamentTeam> CreateTeamWithCaptainAsync(TournamentTeam team, Guid captainId, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                team.CreatedAt = DateTime.UtcNow;
                team.JoinToken = GenerateJoinToken();

                await _dbSet.AddAsync(team, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                var captain = new TournamentPlayer
                {
                    TeamId = team.Id,
                    UserId = captainId,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.TournamentPlayers.AddAsync(captain, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return await GetWithPlayersAsync(team.Id, cancellationToken) ?? team;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<TournamentTeam> AddPlayerToTeamAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default)
        {
            var player = new TournamentPlayer
            {
                TeamId = teamId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.TournamentPlayers.AddAsync(player, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return await GetWithPlayersAsync(teamId, cancellationToken) ?? throw new InvalidOperationException($"Team {teamId} not found");
        }

        public async Task RemovePlayerFromTeamAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default)
        {
            await _context.TournamentPlayers
                .Where(p => p.TeamId == teamId && p.UserId == userId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public async Task UpdatePaymentStatusAsync(Guid teamId, decimal amount, bool isComplete, CancellationToken cancellationToken = default)
        {
            var team = await GetByIdAsync(teamId, cancellationToken);
            if (team == null) throw new InvalidOperationException($"Team {teamId} not found");

            team.PaidAmount = amount;
            team.IsPaymentComplete = isComplete;
            team.UpdatedAt = DateTime.UtcNow;

            if (isComplete && !team.PaymentCompletedAt.HasValue)
                team.PaymentCompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<string> GenerateJoinTokenAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            var team = await GetByIdAsync(teamId, cancellationToken);
            if (team == null) throw new InvalidOperationException($"Team {teamId} not found");

            team.JoinToken = GenerateJoinToken();
            team.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return team.JoinToken;
        }

        private string GenerateJoinToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("=", "").Replace("+", "-").Replace("/", "_")[..12];
        }
    }

    public class TournamentMatchRepository : BaseRepository<TournamentMatch>, ITournamentMatchRepository
    {
        public TournamentMatchRepository(AppDbContext context, ILogger<TournamentMatchRepository> logger) : base(context, logger) { }

        public async Task<TournamentMatch?> GetWithTeamsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(m => m.Team1)
                    .ThenInclude(t => t!.Players)
                        .ThenInclude(p => p.User)
                .Include(m => m.Team2)
                    .ThenInclude(t => t!.Players)
                        .ThenInclude(p => p.User)
                .Include(m => m.Winner)
                .Include(m => m.Tournament)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);
        }

        public async Task<IEnumerable<TournamentMatch>> GetByTournamentIdAsync(Guid tournamentId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(m => m.TournamentId == tournamentId && !m.IsDeleted)
                .Include(m => m.Team1)
                .Include(m => m.Team2)
                .Include(m => m.Winner)
                .OrderBy(m => m.Round)
                .ThenBy(m => m.Position)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TournamentMatch>> GetByRoundAsync(Guid tournamentId, int round, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(m => m.TournamentId == tournamentId && m.Round == round && !m.IsDeleted)
                .Include(m => m.Team1)
                .Include(m => m.Team2)
                .Include(m => m.Winner)
                .OrderBy(m => m.Position)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TournamentMatch>> GetByStatusAsync(TournamentMatchStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(m => m.Status == status && !m.IsDeleted)
                .Include(m => m.Team1)
                .Include(m => m.Team2)
                .Include(m => m.Tournament)
                .OrderBy(m => m.ScheduledAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<TournamentMatch> CreateMatchAsync(TournamentMatch match, CancellationToken cancellationToken = default)
        {
            match.CreatedAt = DateTime.UtcNow;
            await _dbSet.AddAsync(match, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return match;
        }

        public async Task UpdateMatchStatusAsync(Guid matchId, TournamentMatchStatus status, CancellationToken cancellationToken = default)
        {
            var match = await GetByIdAsync(matchId, cancellationToken);
            if (match == null) throw new InvalidOperationException($"Match {matchId} not found");

            match.Status = status;
            match.UpdatedAt = DateTime.UtcNow;

            if (status == TournamentMatchStatus.InProgress && !match.StartedAt.HasValue)
                match.StartedAt = DateTime.UtcNow;
            else if (status == TournamentMatchStatus.Completed && !match.CompletedAt.HasValue)
                match.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateMatchResultAsync(Guid matchId, Guid winnerId, int team1Score, int team2Score, CancellationToken cancellationToken = default)
        {
            var match = await GetByIdAsync(matchId, cancellationToken);
            if (match == null) throw new InvalidOperationException($"Match {matchId} not found");

            match.WinnerId = winnerId;
            match.Team1Score = team1Score;
            match.Team2Score = team2Score;
            match.Status = TournamentMatchStatus.Completed;
            match.CompletedAt = DateTime.UtcNow;
            match.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateServerInfoAsync(Guid matchId, string serverIp, int serverPort, string password, string? serverId = null, CancellationToken cancellationToken = default)
        {
            var match = await GetByIdAsync(matchId, cancellationToken);
            if (match == null) throw new InvalidOperationException($"Match {matchId} not found");

            match.ServerIp = serverIp;
            match.ServerPort = serverPort;
            match.ServerPassword = password;
            match.ExternalServerId = serverId;
            match.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task CompleteMatchAsync(Guid matchId, Guid winnerId, int team1Score, int team2Score, CancellationToken cancellationToken = default)
        {
            var match = await GetByIdAsync(matchId, cancellationToken);
            if (match == null) throw new InvalidOperationException($"Match {matchId} not found");

            match.WinnerId = winnerId;
            match.Team1Score = team1Score;
            match.Team2Score = team2Score;
            match.Status = TournamentMatchStatus.Completed;
            match.CompletedAt = DateTime.UtcNow;
            match.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<TournamentMatch>> GenerateBracketAsync(Guid tournamentId, List<Guid> teamIds, CancellationToken cancellationToken = default)
        {
            var matches = new List<TournamentMatch>();
            var totalRounds = (int)Math.Ceiling(Math.Log2(teamIds.Count));

            // Shuffle teams randomly
            var shuffledTeams = teamIds.OrderBy(x => Guid.NewGuid()).ToList();

            // First round matches
            for (int i = 0; i < shuffledTeams.Count; i += 2)
            {
                var match = new TournamentMatch
                {
                    TournamentId = tournamentId,
                    Round = 1,
                    Position = (i / 2) + 1,
                    Team1Id = shuffledTeams[i],
                    Team2Id = i + 1 < shuffledTeams.Count ? shuffledTeams[i + 1] : null,
                    Status = TournamentMatchStatus.Waiting,
                    CreatedAt = DateTime.UtcNow
                };

                matches.Add(match);
            }

            // Generate subsequent rounds
            var currentRoundMatches = matches.Count / 2;
            for (int round = 2; round <= totalRounds; round++)
            {
                for (int position = 1; position <= currentRoundMatches; position++)
                {
                    var match = new TournamentMatch
                    {
                        TournamentId = tournamentId,
                        Round = round,
                        Position = position,
                        Status = TournamentMatchStatus.Waiting,
                        CreatedAt = DateTime.UtcNow
                    };

                    matches.Add(match);
                }
                currentRoundMatches /= 2;
            }

            await _dbSet.AddRangeAsync(matches, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return matches;
        }

        public async Task<TournamentMatch?> GetByExternalMatchIdAsync(string externalMatchId, CancellationToken cancellationToken = default)
        {
            return await _context.TournamentMatches
                .Include(m => m.Team1)
                .Include(m => m.Team2)
                .Include(m => m.Tournament)
                .FirstOrDefaultAsync(m => m.ExternalMatchId == externalMatchId, cancellationToken);
        }
    }

    public class TournamentPaymentRepository : BaseRepository<TournamentPayment>, ITournamentPaymentRepository
    {
        public TournamentPaymentRepository(AppDbContext context, ILogger<TournamentPaymentRepository> logger) : base(context, logger) { }

        public async Task<IEnumerable<TournamentPayment>> GetByTournamentIdAsync(Guid tournamentId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.TournamentId == tournamentId && !p.IsDeleted)
                .Include(p => p.Team)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TournamentPayment>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.TeamId == teamId && !p.IsDeleted)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<TournamentPayment?> GetByExternalIdAsync(string externalTransactionId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Team)
                .Include(p => p.User)
                .Include(p => p.Tournament)
                .FirstOrDefaultAsync(p => p.ExternalTransactionId == externalTransactionId && !p.IsDeleted, cancellationToken);
        }

        public async Task<TournamentPayment> CreatePaymentAsync(TournamentPayment payment, CancellationToken cancellationToken = default)
        {
            payment.CreatedAt = DateTime.UtcNow;
            await _dbSet.AddAsync(payment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return payment;
        }

        public async Task UpdatePaymentStatusAsync(Guid paymentId, TransactionStatus status, string? externalId = null, CancellationToken cancellationToken = default)
        {
            var payment = await GetByIdAsync(paymentId, cancellationToken);
            if (payment == null) throw new InvalidOperationException($"Payment {paymentId} not found");

            payment.Status = status;
            payment.UpdatedAt = DateTime.UtcNow;

            if (status == TransactionStatus.Completed)
                payment.ProcessedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(externalId))
                payment.ExternalTransactionId = externalId;

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<TournamentPayment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.Status == TransactionStatus.Pending && !p.IsDeleted)
                .Include(p => p.Team)
                .Include(p => p.User)
                .Include(p => p.Tournament)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}