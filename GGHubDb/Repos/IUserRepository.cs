using GGHubDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GGHubDb.Repos
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<User?> GetBySteamIdAsync(string steamId, CancellationToken cancellationToken = default);
        Task<User?> GetByTelegramChatIdAsync(long telegramChatId, CancellationToken cancellationToken = default);
        Task<List<User>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
        Task<bool> IsEmailTakenAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
        Task<bool> IsUsernameTakenAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
        Task UpdateBalanceAsync(Guid userId, decimal amount, CancellationToken cancellationToken = default);
        Task UpdateStatsAsync(Guid userId, bool isWin, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetTopPlayersByRatingAsync(int count, CancellationToken cancellationToken = default);
    }
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting user by email: {Email}", email);
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
        }

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting user by username: {Username}", username);
            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted, cancellationToken);
        }

        public async Task<User?> GetBySteamIdAsync(string steamId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting user by Steam ID: {SteamId}", steamId);
            return await _dbSet.FirstOrDefaultAsync(u => u.SteamId == steamId && !u.IsDeleted, cancellationToken);
        }

        public async Task<User?> GetByTelegramChatIdAsync(long telegramChatId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting user by Telegram chat ID: {TelegramChatId}", telegramChatId);
            return await _dbSet.FirstOrDefaultAsync(u => u.TelegramChatId == telegramChatId && !u.IsDeleted, cancellationToken);
        }

        public async Task<List<User>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting users by ids: {Ids}", string.Join(',', userIds));
            return await _dbSet
                .Where(u => userIds.Contains(u.Id) && !u.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsEmailTakenAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(u => u.Email == email && !u.IsDeleted);
            if (excludeUserId.HasValue)
                query = query.Where(u => u.Id != excludeUserId.Value);

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> IsUsernameTakenAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(u => u.Username == username && !u.IsDeleted);
            if (excludeUserId.HasValue)
                query = query.Where(u => u.Id != excludeUserId.Value);

            return await query.AnyAsync(cancellationToken);
        }

        public async Task UpdateBalanceAsync(Guid userId, decimal amount, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating balance for user {UserId} by {Amount}", userId, amount);

            var user = await GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User not found for balance update: {UserId}", userId);
                throw new InvalidOperationException($"User with ID {userId} not found");
            }

            user.Balance += amount;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateStatsAsync(Guid userId, bool isWin, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating stats for user {UserId}, Win: {IsWin}", userId, isWin);

            var user = await GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User not found for stats update: {UserId}", userId);
                throw new InvalidOperationException($"User with ID {userId} not found");
            }

            if (isWin)
            {
                user.Wins++;
                user.Rating += 25;
            }
            else
            {
                user.Losses++;
                user.Rating = Math.Max(0, user.Rating - 25);
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<User>> GetTopPlayersByRatingAsync(int count, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting top {Count} players by rating", count);
            return await _dbSet
                .Where(u => !u.IsDeleted)
                .OrderByDescending(u => u.Rating)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
    }
    }
