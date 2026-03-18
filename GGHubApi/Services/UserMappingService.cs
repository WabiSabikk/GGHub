using GGHubDb.Repos;
using Microsoft.Extensions.Logging;

namespace GGHubApi.Services
{
    public interface IUserMappingService
    {
        Task<long[]> GetTelegramIdsAsync(Guid[] userIds, CancellationToken cancellationToken = default);
    }

    public class UserMappingService : IUserMappingService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserMappingService> _logger;

        public UserMappingService(IUserRepository userRepository, ILogger<UserMappingService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<long[]> GetTelegramIdsAsync(Guid[] userIds, CancellationToken cancellationToken = default)
        {
            try
            {
                var users = await _userRepository.GetByIdsAsync(userIds, cancellationToken);
                return users
                    .Where(u => u.TelegramChatId.HasValue)
                    .Select(u => u.TelegramChatId!.Value)
                    .ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping telegram ids for users {UserIds}", string.Join(',', userIds));
                return Array.Empty<long>();
            }
        }
    }
}

