using GGHubDb.Models;
using GGHubShared.Enums;
using Microsoft.Extensions.Logging;

namespace GGHubDb.Repos
{
    public interface IMatchConfigRepository : IBaseRepository<DuelFormatConfig>
    {
        Task<DuelFormatConfig?> GetByFormatAsync(DuelFormat format, CancellationToken cancellationToken = default);
        Task<List<DuelFormatConfig>> GetEnabledFormatsAsync(CancellationToken cancellationToken = default);
    }

    public class MatchConfigRepository : BaseRepository<DuelFormatConfig>, IMatchConfigRepository
    {
        public MatchConfigRepository(AppDbContext context, ILogger<MatchConfigRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<DuelFormatConfig?> GetByFormatAsync(DuelFormat format, CancellationToken cancellationToken = default)
        {
            return await FirstOrDefaultAsync(c => c.Format == format && c.IsEnabled, cancellationToken);
        }

        public async Task<List<DuelFormatConfig>> GetEnabledFormatsAsync(CancellationToken cancellationToken = default)
        {
            var configs = await FindAsync(c => c.IsEnabled, cancellationToken);
            return configs.ToList();
        }
    }
}
