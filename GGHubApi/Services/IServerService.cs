using DatHostApi;
using GGHubDb.Models;
using GGHubDb.Repos;
using GGHubShared.Enums;
using GGHubShared.Models;

namespace GGHubApi.Services
{
    public interface IServerService
    {
        Task<string> GetOptimalLocationAsync(string? userCountry, string? preferredRegion = null, CancellationToken cancellationToken = default);
        Task<string> GetFallbackLocationAsync(string primaryLocation, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<ServerRegionConfig>>> GetAvailableRegionsAsync(CancellationToken cancellationToken = default);
        bool IsPremiumEnvironment();
         Task<GameServer?> FindAvailableServerAsync(string location, int requiredSlots, CancellationToken cancellationToken = default);
    }

    public class ServerService : IServerService
    {
        private readonly IServerRepository _regionRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ServerService> _logger;
        private readonly IGameServerService _gameServerService;
        public ServerService(
            IServerRepository regionRepository,
            IWebHostEnvironment environment,
            IGameServerService gameServerService,
            ILogger<ServerService> logger)
        {
            _regionRepository = regionRepository;
            _environment = environment;
            _logger = logger;
            _gameServerService = gameServerService;
        }

        public async Task<string> GetOptimalLocationAsync(string? userCountry, string? preferredRegion = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!string.IsNullOrEmpty(preferredRegion))
                {
                    var region = await _regionRepository.GetByRegionAsync(preferredRegion, cancellationToken);
                    if (region != null)
                        return region.PrimaryLocation;
                }

                if (!string.IsNullOrEmpty(userCountry))
                {
                    var countryRegion = await _regionRepository.GetByCountryAsync(userCountry, cancellationToken);
                    if (countryRegion != null)
                        return countryRegion.PrimaryLocation;
                }

                _logger.LogWarning("No optimal location found for country: {Country}, region: {Region}", userCountry, preferredRegion);
                return "stockholm";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting optimal location");
                return "stockholm";
            }
        }

        public async Task<string> GetFallbackLocationAsync(string primaryLocation, CancellationToken cancellationToken = default)
        {
            try
            {
                var all = await _regionRepository.GetAvailableRegionsAsync(cancellationToken);
                var region = all.FirstOrDefault(r => r.PrimaryLocation == primaryLocation);
                if (region != null && !string.IsNullOrEmpty(region.FallbackLocations))
                {
                    var fallback = region.FallbackLocations.Split(',').First().Trim();
                    return fallback;
                }

                return "stockholm";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fallback location for: {PrimaryLocation}", primaryLocation);
                return "stockholm";
            }
        }

        public async Task<ApiResponse<List<ServerRegionConfig>>> GetAvailableRegionsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var regions = await _regionRepository.GetAvailableRegionsAsync(cancellationToken);
                return new ApiResponse<List<ServerRegionConfig>> { Success = true, Data = regions };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available regions");
                return new ApiResponse<List<ServerRegionConfig>> { Success = false, Code = ErrorCode.ServerError, Errors = { ex.Message } };
            }
        }

        public async Task<GameServer?> FindAvailableServerAsync(string location, int requiredSlots, CancellationToken cancellationToken = default)
        {
           var candidates = await  _regionRepository.FindAvailableServerAsync(location, requiredSlots);
            foreach (var server in candidates)
            {
                try
                {
                    var info = await _gameServerService.GetGameServerAsync(server.ExternalServerId!, cancellationToken);
                    if (info != null &&
                        string.Equals(info.Location, location, StringComparison.OrdinalIgnoreCase) &&
                        info.Cs2Settings?.Slots >= requiredSlots)
                    {
                        return server;
                    }
                }
                catch
                {
                    server.IsDeleted = true;
                    //throw;
                }
            }

            return null;
        }


        public bool IsPremiumEnvironment()
        {
            return _environment.IsProduction();
        }
    }
}
