using GGHubDb.Models;
using GGHubShared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GGHubDb.Repos
{
    public interface IDuelRepository : IBaseRepository<Duel>
    {
        Task<Duel?> GetWithParticipantsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Duel?> GetWithMapsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Duel?> GetFullDuelAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Duel>> GetByStatusAsync(DuelStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Duel>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Duel>> GetAvailableDuelsAsync(CancellationToken cancellationToken = default);
        Task<bool> IsUserInDuelAsync(Guid userId, Guid duelId, CancellationToken cancellationToken = default);
        Task<int> GetUserActiveDuelsCountAsync(Guid userId, CancellationToken cancellationToken = default);
        Task UpdateStatusAsync(Guid duelId, DuelStatus status, CancellationToken cancellationToken = default);
        Task<(IEnumerable<Duel> Items, int TotalCount)> GetPagedWithFiltersAsync(
            int pageNumber,
            int pageSize,
            DuelFormat? format = null,
            DuelStatus? status = null,
            Guid? userId = null,
            CancellationToken cancellationToken = default);

        Task<Duel> CreateDuelWithMapsAndParticipantAsync(Duel duel, List<string> maps, Guid creatorId, CancellationToken cancellationToken = default);
        Task<Duel> AddParticipantToDuelAsync(Guid duelId, Guid userId, int team, CancellationToken cancellationToken = default);
        Task RemoveParticipantFromDuelAsync(Guid duelId, Guid userId, CancellationToken cancellationToken = default);
        Task CompleteDuelAsync(Guid duelId, Guid winnerId, CancellationToken cancellationToken = default);
        Task<string?> GenerateInviteLinkAsync(Guid duelId, CancellationToken cancellationToken = default);
        Task SetParticipantReadyAsync(Guid duelId, Guid userId, bool isReady, CancellationToken cancellationToken = default);
        Task<bool> AreAllParticipantsReadyAsync(Guid duelId, CancellationToken cancellationToken = default);
        Task SetParticipantPaidAsync(Guid duelId, Guid userId, bool hasPaid, CancellationToken cancellationToken = default);
        Task<bool> HaveAllParticipantsPaidAsync(Guid duelId, CancellationToken cancellationToken = default);
        Task AddGameServerAsync(GameServer gameServer, CancellationToken cancellationToken = default);

        Task UpdateGameServerForDuelAsync(Guid oldDuelId, Guid newDuelId, string newPassword, CancellationToken cancellationToken = default);

        Task UpdateGameServerMatchIdAsync(Guid duelId, string matchId, CancellationToken cancellationToken = default);

        Task UpdateGameServerStatusAsync(Guid duelId, ServerStatus status, CancellationToken cancellationToken = default);

        Task UpdateParticipantStatsAsync(Guid participantId, int kills, int deaths, int assists, int score, CancellationToken cancellationToken = default);

        Task<Duel?> GetByExternalMatchIdAsync(string externalMatchId, CancellationToken cancellationToken = default);
    }

    public class DuelRepository : BaseRepository<Duel>, IDuelRepository
    {
        public DuelRepository(AppDbContext context, ILogger<DuelRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<Duel?> GetWithParticipantsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting duel with participants: {DuelId}", id);
            return await _dbSet
                .Include(d => d.Participants)
                    .ThenInclude(p => p.User)
                .Include(d => d.Creator)
                .Include(d => d.Winner)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);
        }

        public async Task<Duel?> GetWithMapsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting duel with maps: {DuelId}", id);
            return await _dbSet
                .Include(d => d.Maps)
                .Include(d => d.Creator)
                .Include(d => d.Winner)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);
        }

        public async Task<Duel?> GetFullDuelAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting full duel data: {DuelId}", id);
            try
            {

                return await _dbSet
                    .Include(d => d.Participants)
                        .ThenInclude(p => p.User)
                    .Include(d => d.Maps)
                    .Include(d => d.Creator)
                    .Include(d => d.Winner)
                    .Include(d => d.GameServer)
                    .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Duel>> GetByStatusAsync(DuelStatus status, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting duels by status: {Status}", status);
            return await _dbSet
                .Where(d => d.Status == status && !d.IsDeleted)
                .Include(d => d.Creator)
                .Include(d => d.Participants)
                    .ThenInclude(p => p.User)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Duel>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting duels for user: {UserId}", userId);
            return await _dbSet
                .Where(d => !d.IsDeleted &&
                           (d.CreatedBy == userId || d.Participants.Any(p => p.UserId == userId)) &&
                           d.Status != DuelStatus.Completed &&
                           d.Status != DuelStatus.Cancelled)
                .Include(d => d.Creator)
                .Include(d => d.Winner)
                .Include(d => d.GameServer)
                .Include(d => d.Participants)
                    .ThenInclude(p => p.User)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Duel>> GetAvailableDuelsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting available duels");
            return await _dbSet
                .Where(d => !d.IsDeleted &&
                           d.Status == DuelStatus.WaitingForPlayers &&
                           d.CurrentParticipants < d.MaxParticipants)
                .Include(d => d.Creator)
                .Include(d => d.Participants)
                    .ThenInclude(p => p.User)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsUserInDuelAsync(Guid userId, Guid duelId, CancellationToken cancellationToken = default)
        {
            return await _context.DuelParticipants
                .AnyAsync(dp => dp.UserId == userId && dp.DuelId == duelId && !dp.IsDeleted, cancellationToken);
        }

        public async Task<int> GetUserActiveDuelsCountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting active duels count for user: {UserId}", userId);
            return await _dbSet
                .Where(d => !d.IsDeleted &&
                           (d.CreatedBy == userId || d.Participants.Any(p => p.UserId == userId)) &&
                           (d.Status == DuelStatus.WaitingForPlayers ||
                            d.Status == DuelStatus.PaymentPending ||
                            d.Status == DuelStatus.Starting ||
                            d.Status == DuelStatus.InProgress))
                .CountAsync(cancellationToken);
        }

        public async Task UpdateStatusAsync(Guid duelId, DuelStatus status, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating duel status: {DuelId} to {Status}", duelId, status);

            var duel = await GetByIdAsync(duelId, cancellationToken);
            if (duel == null)
            {
                _logger.LogWarning("Duel not found for status update: {DuelId}", duelId);
                throw new InvalidOperationException($"Duel with ID {duelId} not found");
            }

            duel.Status = status;
            duel.UpdatedAt = DateTime.UtcNow;

            if (status == DuelStatus.InProgress && !duel.StartedAt.HasValue)
                duel.StartedAt = DateTime.UtcNow;
            else if (status == DuelStatus.Completed && !duel.CompletedAt.HasValue)
                duel.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Duel> Items, int TotalCount)> GetPagedWithFiltersAsync(
            int pageNumber,
            int pageSize,
            DuelFormat? format = null,
            DuelStatus? status = null,
            Guid? userId = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting paged duels with filters - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

            var query = _dbSet
                .Where(d => !d.IsDeleted)
                .Include(d => d.Creator)
                .Include(d => d.Winner)
                .Include(d => d.Participants)
                    .ThenInclude(p => p.User)
                .AsQueryable();

            if (format.HasValue)
                query = query.Where(d => d.Format == format.Value);

            if (status.HasValue)
                query = query.Where(d => d.Status == status.Value);

            if (userId.HasValue)
                query = query.Where(d => d.CreatedBy == userId.Value ||
                                       d.Participants.Any(p => p.UserId == userId.Value));

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        // New methods for proper encapsulation
        public async Task<Duel> CreateDuelWithMapsAndParticipantAsync(Duel duel, List<string> maps, Guid creatorId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating duel with maps and participant: {DuelId}", duel.Id);

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Add duel
                    duel.CreatedAt = DateTime.UtcNow;
                    await _dbSet.AddAsync(duel, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    // Add maps
                    foreach (var (mapName, index) in maps.Select((map, i) => (map, i)))
                    {
                        var duelMap = new DuelMap
                        {
                            DuelId = duel.Id,
                            MapName = mapName,
                            Order = index + 1,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _context.DuelMaps.AddAsync(duelMap, cancellationToken);
                    }

                    // Add creator as participant
                    var participant = new DuelParticipant
                    {
                        DuelId = duel.Id,
                        UserId = creatorId,
                        Team = 1,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.DuelParticipants.AddAsync(participant, cancellationToken);

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    // Return full duel data
                    return await GetFullDuelAsync(duel.Id, cancellationToken) ?? duel;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }

        public async Task<Duel> AddParticipantToDuelAsync(Guid duelId, Guid userId, int team, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Adding participant to duel: {DuelId}, User: {UserId}, Team: {Team}", duelId, userId, team);

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Add participant
                    var participant = new DuelParticipant
                    {
                        DuelId = duelId,
                        UserId = userId,
                        Team = team,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.DuelParticipants.AddAsync(participant, cancellationToken);

                    // Update duel participant count and status
                    var duel = await GetByIdAsync(duelId, cancellationToken);
                    if (duel != null)
                    {
                        duel.CurrentParticipants++;
                        if (duel.CurrentParticipants == duel.MaxParticipants)
                        {
                            duel.Status = DuelStatus.PaymentPending;
                        }
                        duel.UpdatedAt = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    // Return updated duel with full data
                    return await GetFullDuelAsync(duelId, cancellationToken) ?? duel!;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }

        public async Task RemoveParticipantFromDuelAsync(Guid duelId, Guid userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Removing participant from duel: {DuelId}, User: {UserId}", duelId, userId);

            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Remove participant
                    await _context.DuelParticipants
                        .Where(p => p.DuelId == duelId && p.UserId == userId && !p.IsDeleted)
                        .ExecuteDeleteAsync(cancellationToken);

                    // Update duel participant count and status
                    var duel = await GetByIdAsync(duelId, cancellationToken);
                    if (duel != null)
                    {
                        duel.CurrentParticipants--;
                        if (duel.CurrentParticipants == 0)
                        {
                            duel.Status = DuelStatus.Cancelled;
                        }
                        else if (duel.Status == DuelStatus.PaymentPending)
                        {
                            duel.Status = DuelStatus.WaitingForPlayers;
                        }
                        duel.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync(cancellationToken);
                    }

                    await transaction.CommitAsync(cancellationToken);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }

        public async Task CompleteDuelAsync(Guid duelId, Guid winnerId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Completing duel: {DuelId} with winner: {WinnerId}", duelId, winnerId);

            var duel = await GetByIdAsync(duelId, cancellationToken);
            if (duel == null)
            {
                throw new InvalidOperationException($"Duel with ID {duelId} not found");
            }

            duel.WinnerId = winnerId;
            duel.Status = DuelStatus.Completed;
            duel.CompletedAt = DateTime.UtcNow;
            duel.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<string?> GenerateInviteLinkAsync(Guid duelId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Generating invite link for duel: {DuelId}", duelId);

            var duel = await GetByIdAsync(duelId, cancellationToken);
            if (duel == null)
            {
                return null;
            }

            var inviteCode = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("=", "").Replace("+", "-").Replace("/", "_")[..8];

            duel.InviteLink = inviteCode;
            duel.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return inviteCode;
        }

        public async Task SetParticipantReadyAsync(Guid duelId, Guid userId, bool isReady, CancellationToken cancellationToken = default)
        {
            var participant = await _context.DuelParticipants
                .FirstOrDefaultAsync(p => p.DuelId == duelId && p.UserId == userId && !p.IsDeleted, cancellationToken);
            if (participant == null) return;

            participant.IsReady = isReady;
            participant.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> AreAllParticipantsReadyAsync(Guid duelId, CancellationToken cancellationToken = default)
        {
            return await _context.DuelParticipants
                .Where(p => p.DuelId == duelId && !p.IsDeleted)
                .AllAsync(p => p.IsReady, cancellationToken);
        }

        public async Task SetParticipantPaidAsync(Guid duelId, Guid userId, bool hasPaid, CancellationToken cancellationToken = default)
        {
            var participant = await _context.DuelParticipants
                .FirstOrDefaultAsync(p => p.DuelId == duelId && p.UserId == userId && !p.IsDeleted, cancellationToken);
            if (participant == null) return;

            
            participant.HasPaid = hasPaid;
            participant.PaidAt = hasPaid ? DateTime.UtcNow : null;
            participant.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> HaveAllParticipantsPaidAsync(Guid duelId, CancellationToken cancellationToken = default)
        {
            return await _context.DuelParticipants
                .Where(p => p.DuelId == duelId && !p.IsDeleted)
                .AllAsync(p => p.HasPaid, cancellationToken);
        }

        public async Task AddGameServerAsync(GameServer gameServer, CancellationToken cancellationToken = default)
        {
            await _context.GameServers.AddAsync(gameServer, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task UpdateGameServerForDuelAsync(Guid oldDuelId, Guid newDuelId, string newPassword, CancellationToken cancellationToken = default)
        {
            var gameServer = await _context.GameServers
                .FirstOrDefaultAsync(gs => gs.DuelId == oldDuelId && !gs.IsDeleted, cancellationToken);

            if (gameServer != null)
            {
                gameServer.DuelId = newDuelId;
                gameServer.Password = newPassword;
                gameServer.Status = ServerStatus.Stopped;
                gameServer.StartedAt = null;
                gameServer.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task UpdateGameServerMatchIdAsync(Guid duelId, string matchId, CancellationToken cancellationToken = default)
        {
            var gameServer = await _context.GameServers
                .FirstOrDefaultAsync(gs => gs.DuelId == duelId && !gs.IsDeleted, cancellationToken);

            if (gameServer != null)
            {
                gameServer.ExternalMatchId = matchId;
                gameServer.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task UpdateGameServerStatusAsync(Guid duelId, ServerStatus status, CancellationToken cancellationToken = default)
        {
            var gameServer = await _context.GameServers
                .FirstOrDefaultAsync(gs => gs.DuelId == duelId && !gs.IsDeleted, cancellationToken);

            if (gameServer != null)
            {
                gameServer.Status = status;
                if (status == ServerStatus.Running && !gameServer.StartedAt.HasValue)
                    gameServer.StartedAt = DateTime.UtcNow;
                else if (status == ServerStatus.Stopped)
                    gameServer.StoppedAt = DateTime.UtcNow;

                gameServer.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task UpdateParticipantStatsAsync(Guid participantId, int kills, int deaths, int assists, int score, CancellationToken cancellationToken = default)
        {
            var participant = await _context.DuelParticipants
                .FirstOrDefaultAsync(p => p.Id == participantId && !p.IsDeleted, cancellationToken);

            if (participant != null)
            {
                participant.Kills = kills;
                participant.Deaths = deaths;
                participant.Assists = assists;
                participant.Score = score;
                participant.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<Duel?> GetByExternalMatchIdAsync(string externalMatchId, CancellationToken cancellationToken = default)
        {
            return await _context.Duels
                .Include(d => d.GameServer)
                .Include(d => d.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(d => d.GameServer != null && d.GameServer.ExternalMatchId == externalMatchId, cancellationToken);
        }
    }
}