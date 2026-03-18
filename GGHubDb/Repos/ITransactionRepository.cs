using GGHubDb.Models;
using GGHubShared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GGHubDb.Repos
{
    public interface ITransactionRepository : IBaseRepository<Transaction>
    {
        Task<Transaction?> GetByExternalIdAsync(string externalTransactionId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Transaction>> GetByDuelIdAsync(Guid duelId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType type, CancellationToken cancellationToken = default);
        Task<IEnumerable<Transaction>> GetPendingTransactionsAsync(CancellationToken cancellationToken = default);
        Task<decimal> GetUserBalanceAsync(Guid userId, CancellationToken cancellationToken = default);
        Task UpdateStatusAsync(Guid transactionId, TransactionStatus status, string? errorMessage = null, CancellationToken cancellationToken = default);
        Task<(IEnumerable<Transaction> Items, int TotalCount)> GetPagedByUserAsync(
            Guid userId,
            int pageNumber,
            int pageSize,
            TransactionType? type = null,
            CancellationToken cancellationToken = default);

        Task<Transaction> CreateEntryFeeTransactionAsync(Guid userId, Guid duelId, decimal amount, string duelTitle, CancellationToken cancellationToken = default);
        Task<Transaction> CreatePrizeTransactionAsync(Guid userId, Guid duelId, decimal amount, string duelTitle, CancellationToken cancellationToken = default);
        Task<Transaction> CreateRefundTransactionAsync(Guid userId, Guid? duelId, decimal amount, string reason, CancellationToken cancellationToken = default);
        Task CompleteTransactionAsync(Guid transactionId, string? externalTransactionId = null, CancellationToken cancellationToken = default);
    }

    public class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(AppDbContext context, ILogger<TransactionRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<Transaction?> GetByExternalIdAsync(string externalTransactionId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting transaction by external ID: {ExternalId}", externalTransactionId);
            return await _dbSet
                .Include(t => t.User)
                .Include(t => t.Duel)
                .FirstOrDefaultAsync(t => t.ExternalTransactionId == externalTransactionId && !t.IsDeleted, cancellationToken);
        }

        public async Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting transactions for user: {UserId}", userId);
            return await _dbSet
                .Where(t => t.UserId == userId && !t.IsDeleted)
                .Include(t => t.Duel)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Transaction>> GetByDuelIdAsync(Guid duelId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting transactions for duel: {DuelId}", duelId);
            return await _dbSet
                .Where(t => t.DuelId == duelId && !t.IsDeleted)
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType type, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting transactions by type: {Type}", type);
            return await _dbSet
                .Where(t => t.Type == type && !t.IsDeleted)
                .Include(t => t.User)
                .Include(t => t.Duel)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Transaction>> GetPendingTransactionsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting pending transactions");
            return await _dbSet
                .Where(t => t.Status == TransactionStatus.Pending && !t.IsDeleted)
                .Include(t => t.User)
                .Include(t => t.Duel)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<decimal> GetUserBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Calculating user balance: {UserId}", userId);

            var deposits = await _dbSet
                .Where(t => t.UserId == userId &&
                           !t.IsDeleted &&
                           t.Status == TransactionStatus.Completed &&
                           (t.Type == TransactionType.Deposit || t.Type == TransactionType.Prize || t.Type == TransactionType.Refund))
                .SumAsync(t => t.Amount, cancellationToken);

            var withdrawals = await _dbSet
                .Where(t => t.UserId == userId &&
                           !t.IsDeleted &&
                           t.Status == TransactionStatus.Completed &&
                           (t.Type == TransactionType.EntryFee || t.Type == TransactionType.Withdrawal))
                .SumAsync(t => t.Amount, cancellationToken);

            return deposits - withdrawals;
        }

        public async Task UpdateStatusAsync(Guid transactionId, TransactionStatus status, string? errorMessage = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating transaction status: {TransactionId} to {Status}", transactionId, status);

            var transaction = await GetByIdAsync(transactionId, cancellationToken);
            if (transaction == null)
            {
                _logger.LogWarning("Transaction not found for status update: {TransactionId}", transactionId);
                throw new InvalidOperationException($"Transaction with ID {transactionId} not found");
            }

            transaction.Status = status;
            transaction.UpdatedAt = DateTime.UtcNow;

            if (status == TransactionStatus.Completed)
                transaction.ProcessedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(errorMessage))
                transaction.ErrorMessage = errorMessage;

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Transaction> Items, int TotalCount)> GetPagedByUserAsync(
            Guid userId,
            int pageNumber,
            int pageSize,
            TransactionType? type = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting paged transactions for user {UserId} - Page: {PageNumber}, Size: {PageSize}", userId, pageNumber, pageSize);

            var query = _dbSet
                .Where(t => t.UserId == userId && !t.IsDeleted)
                .Include(t => t.Duel)
                .AsQueryable();

            if (type.HasValue)
                query = query.Where(t => t.Type == type.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        // New methods for proper encapsulation
        public async Task<Transaction> CreateEntryFeeTransactionAsync(Guid userId, Guid duelId, decimal amount, string duelTitle, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating entry fee transaction: User: {UserId}, Duel: {DuelId}, Amount: {Amount}", userId, duelId, amount);

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Create transaction
                var entryFeeTransaction = new Transaction
                {
                    UserId = userId,
                    DuelId = duelId,
                    Type = TransactionType.EntryFee,
                    Amount = amount,
                    Status = TransactionStatus.Completed,
                    Description = $"Entry fee for duel: {duelTitle}",
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow
                };

                await _dbSet.AddAsync(entryFeeTransaction, cancellationToken);

                // Update user balance
                await _context.Users
                    .Where(u => u.Id == userId && !u.IsDeleted)
                    .ExecuteUpdateAsync(u => u.SetProperty(x => x.Balance, x => x.Balance - amount)
                                              .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return entryFeeTransaction;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<Transaction> CreatePrizeTransactionAsync(Guid userId, Guid duelId, decimal amount, string duelTitle, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating prize transaction: User: {UserId}, Duel: {DuelId}, Amount: {Amount}", userId, duelId, amount);

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
             
                var prizeTransaction = new Transaction
                {
                    UserId = userId,
                    DuelId = duelId,
                    Type = TransactionType.Prize,
                    Amount = amount,
                    Status = TransactionStatus.Completed,
                    Description = $"Prize from duel: {duelTitle}",
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow
                };

                await _dbSet.AddAsync(prizeTransaction, cancellationToken);

            
                await _context.Users
                    .Where(u => u.Id == userId && !u.IsDeleted)
                    .ExecuteUpdateAsync(u => u.SetProperty(x => x.Balance, x => x.Balance + amount)
                                              .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return prizeTransaction;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<Transaction> CreateRefundTransactionAsync(Guid userId, Guid? duelId, decimal amount, string reason, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating refund transaction: User: {UserId}, Amount: {Amount}, Reason: {Reason}", userId, amount, reason);

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
            
                var refundTransaction = new Transaction
                {
                    UserId = userId,
                    DuelId = duelId,
                    Type = TransactionType.Refund,
                    Amount = amount,
                    Status = TransactionStatus.Completed,
                    Description = $"Refund: {reason}",
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow
                };

                await _dbSet.AddAsync(refundTransaction, cancellationToken);

             
                await _context.Users
                    .Where(u => u.Id == userId && !u.IsDeleted)
                    .ExecuteUpdateAsync(u => u.SetProperty(x => x.Balance, x => x.Balance + amount)
                                              .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return refundTransaction;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task CompleteTransactionAsync(Guid transactionId, string? externalTransactionId = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Completing transaction: {TransactionId}", transactionId);

            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var dbTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var transaction = await GetByIdAsync(transactionId, cancellationToken);

                  
                    if (transaction == null)
                        throw new InvalidOperationException($"Transaction with ID {transactionId} not found");

                    if (transaction.Status != TransactionStatus.Pending)
                        throw new InvalidOperationException($"Transaction {transactionId} is not in pending status");

                   
                    if (transaction.Type == TransactionType.Withdrawal)
                    {
                        var currentBalance = await _context.Users
                            .Where(u => u.Id == transaction.UserId && !u.IsDeleted)
                            .Select(u => u.Balance)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (currentBalance < transaction.Amount)
                            throw new InvalidOperationException("Insufficient balance for withdrawal");
                    }

                 
                    transaction.Status = TransactionStatus.Completed;
                    transaction.ProcessedAt = DateTime.UtcNow;
                    transaction.UpdatedAt = DateTime.UtcNow;

                    if (!string.IsNullOrEmpty(externalTransactionId))
                        transaction.ExternalTransactionId = externalTransactionId;

                   
                    decimal balanceChange = transaction.Type == TransactionType.Deposit
                        ? transaction.Amount
                        : -transaction.Amount;

                    var affectedRows = await _context.Users
                        .Where(u => u.Id == transaction.UserId && !u.IsDeleted)
                        .ExecuteUpdateAsync(u => u
                            .SetProperty(x => x.Balance, x => x.Balance + balanceChange)
                            .SetProperty(x => x.UpdatedAt, DateTime.UtcNow),
                            cancellationToken);

                    if (affectedRows == 0)
                        throw new InvalidOperationException("User not found or was deleted");

                   
                    await _context.SaveChangesAsync(cancellationToken);
                    await dbTransaction.CommitAsync(cancellationToken);

                    _logger.LogInformation("Transaction {TransactionId} completed successfully", transactionId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to complete transaction {TransactionId}: {ErrorMessage}", transactionId, ex.Message);
                    await dbTransaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
    }
    }
}