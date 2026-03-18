using GGHubDb.Models;
using GGHubShared.Enums;
using GGHubShared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GGHubDb.Services
{
    public interface IMatchMetricsService
    {
        Task<ApiResponse<GlobalMatchMetricsDto>> GetGlobalMetricsAsync(CancellationToken cancellationToken = default);
        Task<ApiResponse<UserMatchMetricsDto>> GetUserMetricsAsync(Guid userId, CancellationToken cancellationToken = default);
    }

    public class MatchMetricsService : IMatchMetricsService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MatchMetricsService> _logger;

        public MatchMetricsService(AppDbContext context, ILogger<MatchMetricsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<GlobalMatchMetricsDto>> GetGlobalMetricsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var deposits = await _context.Transactions
                    .Where(t => !t.IsDeleted && t.Status == TransactionStatus.Completed && t.Type == TransactionType.EntryFee && t.DuelId != null)
                    .GroupBy(t => t.DuelId)
                    .Select(g => new { Total = g.Sum(x => x.Amount) })
                    .ToListAsync(cancellationToken);

                var totalMatches = deposits.Count;
                var averageDeposit = totalMatches > 0 ? deposits.Average(d => d.Total) : 0m;

                return new ApiResponse<GlobalMatchMetricsDto>
                {
                    Success = true,
                    Data = new GlobalMatchMetricsDto
                    {
                        TotalMatches = totalMatches,
                        AverageDeposit = averageDeposit
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting global match metrics");
                return new ApiResponse<GlobalMatchMetricsDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving metrics",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<UserMatchMetricsDto>> GetUserMetricsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var deposits = await _context.Transactions
                    .Where(t => !t.IsDeleted && t.Status == TransactionStatus.Completed && t.Type == TransactionType.EntryFee && t.UserId == userId)
                    .Select(t => t.Amount)
                    .ToListAsync(cancellationToken);

                var matchesPlayed = deposits.Count;
                var averageDeposit = matchesPlayed > 0 ? deposits.Average() : 0m;

                return new ApiResponse<UserMatchMetricsDto>
                {
                    Success = true,
                    Data = new UserMatchMetricsDto
                    {
                        MatchesPlayed = matchesPlayed,
                        AverageDeposit = averageDeposit
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics for user {UserId}", userId);
                return new ApiResponse<UserMatchMetricsDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving metrics",
                    Errors = { ex.Message }
                };
            }
        }
    }
}
