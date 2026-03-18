using GGHubDb.Models;
using GGHubShared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GGHubDb.Repos
{
    public interface IServerRepository : IBaseRepository<ServerRegionConfig>
    {
        Task<ServerRegionConfig?> GetByRegionAsync(string regionCode, CancellationToken cancellationToken = default);
        Task<ServerRegionConfig?> GetByCountryAsync(string countryCode, CancellationToken cancellationToken = default);
        Task<List<ServerRegionConfig>> GetAvailableRegionsAsync(CancellationToken cancellationToken = default);
        Task<List<GameServer>> FindAvailableServerAsync(string location, int requiredSlots, CancellationToken cancellationToken = default);
    }

    public class ServerRegionRepository : BaseRepository<ServerRegionConfig>, IServerRepository
    {
        public ServerRegionRepository(AppDbContext context, ILogger<ServerRegionRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<ServerRegionConfig?> GetByRegionAsync(string regionCode, CancellationToken cancellationToken = default)
        {
            return await FirstOrDefaultAsync(r => r.RegionCode == regionCode && r.IsEnabled, cancellationToken);
        }

        public async Task<ServerRegionConfig?> GetByCountryAsync(string countryCode, CancellationToken cancellationToken = default)
        {
            return await FirstOrDefaultAsync(r => r.CountryCodes.Contains(countryCode) && r.IsEnabled, cancellationToken);
        }

        public async Task<List<ServerRegionConfig>> GetAvailableRegionsAsync(CancellationToken cancellationToken = default)
        {
            var regions = await FindAsync(r => r.IsEnabled, cancellationToken);
            return regions.ToList();
        }

        public async Task<List<GameServer>> FindAvailableServerAsync(string location, int requiredSlots, CancellationToken cancellationToken = default)
        {

            var allowedStatuses = new[] { DuelStatus.Completed, DuelStatus.Cancelled };

            return await _context.GameServers
                .Include(gs => gs.Duel)
                .Where(gs => !gs.IsDeleted &&
                            gs.Status == ServerStatus.Stopped &&
                            allowedStatuses.Contains(gs.Duel.Status) &&
                            !string.IsNullOrEmpty(gs.ExternalServerId))
                .OrderBy(gs => gs.UpdatedAt)
                .ToListAsync(cancellationToken) ?? new();
        }
    }
}
