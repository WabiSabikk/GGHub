using GGHubDb.Models;
using GGHubDb.Repos;
using GGHubShared.Enums;
using GGHubShared.Helpers;
using GGHubShared.Models;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace GGHubDb.Services
{
    public interface IUserService
    {
        Task<ApiResponse<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<UserDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<ApiResponse<UserDto>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<ApiResponse<UserDto>> GetByTelegramChatIdAsync(long telegramChatId, CancellationToken cancellationToken = default);
        Task<ApiResponse<UserDto>> CreateUserAsync(string username, string email, string? avatar = null, string? country = null, string? passwordHash = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<UserDto>> UpdateUserAsync(Guid id, string? username = null, string? email = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> UpdateBalanceAsync(Guid userId, decimal amount, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> UpdateStatsAsync(Guid userId, bool isWin, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> LinkSteamAccountAsync(Guid userId, string steamId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> LinkTelegramAccountAsync(Guid userId, string telegramUsername, long telegramChatId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ActivatePrimeAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeactivatePrimeAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<UserDto>>> GetTopPlayersByRatingAsync(int count = 10, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> IsEmailAvailableAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> IsUsernameAvailableAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
    }
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IMapper mapper, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id, cancellationToken);
                if (user == null)
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Code = ErrorCode.UserNotFound
                    };
                }

                return new ApiResponse<UserDto>
                {
                    Success = true,
                    Data = _mapper.Map<UserDto>(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", id);
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<UserDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
                if (user == null)
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Code = ErrorCode.UserNotFound
                    };
                }

                return new ApiResponse<UserDto>
                {
                    Success = true,
                    Data = _mapper.Map<UserDto>(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email: {Email}", email);
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<UserDto>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
                if (user == null)
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Code = ErrorCode.UserNotFound
                    };
                }

                return new ApiResponse<UserDto>
                {
                    Success = true,
                    Data = _mapper.Map<UserDto>(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by username: {Username}", username);
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<UserDto>> GetByTelegramChatIdAsync(long telegramChatId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByTelegramChatIdAsync(telegramChatId, cancellationToken);
                if (user == null)
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Code = ErrorCode.UserNotFound
                    };
                }

                return new ApiResponse<UserDto>
                {
                    Success = true,
                    Data = _mapper.Map<UserDto>(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by Telegram chat ID: {TelegramChatId}", telegramChatId);
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<UserDto>> CreateUserAsync(string username, string email,string? avatar = null, string? country = null, string? passwordHash = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (await _userRepository.IsEmailTakenAsync(email, cancellationToken: cancellationToken))
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "Email is already taken"
                    };
                }

                if (await _userRepository.IsUsernameTakenAsync(username, cancellationToken: cancellationToken))
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "Username is already taken"
                    };
                }

                var user = new User
                {
                    Username = username,
                    Email = email,
                    AvatarUrl = avatar ?? Constants.DEFAULT_AVATAR,
                    Country = country,
                    PasswordHash = passwordHash,
                    Role = UserRole.User
                };

                user = await _userRepository.AddAsync(user, cancellationToken);
                _logger.LogInformation("Created new user: {UserId} with username: {Username}", user.Id, username);

                return new ApiResponse<UserDto>
                {
                    Success = true,
                    Data = _mapper.Map<UserDto>(user),
                    Message = "User created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user with username: {Username} and email: {Email}", username, email);
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "An error occurred while creating user",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<UserDto>> UpdateUserAsync(Guid id, string? username = null, string? email = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id, cancellationToken);
                if (user == null)
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Code = ErrorCode.UserNotFound
                    };
                }

                if (!string.IsNullOrEmpty(email) && email != user.Email)
                {
                    if (await _userRepository.IsEmailTakenAsync(email, id, cancellationToken))
                    {
                        return new ApiResponse<UserDto>
                        {
                            Success = false,
                            Message = "Email is already taken"
                        };
                    }
                    user.Email = email;
                }

                if (!string.IsNullOrEmpty(username) && username != user.Username)
                {
                    if (await _userRepository.IsUsernameTakenAsync(username, id, cancellationToken))
                    {
                        return new ApiResponse<UserDto>
                        {
                            Success = false,
                            Message = "Username is already taken"
                        };
                    }
                    user.Username = username;
                }

                user = await _userRepository.UpdateAsync(user, cancellationToken);
                _logger.LogInformation("Updated user: {UserId}", id);

                return new ApiResponse<UserDto>
                {
                    Success = true,
                    Data = _mapper.Map<UserDto>(user),
                    Message = "User updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", id);
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "An error occurred while updating user",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateBalanceAsync(Guid userId, decimal amount, CancellationToken cancellationToken = default)
        {
            try
            {
                await _userRepository.UpdateBalanceAsync(userId, amount, cancellationToken);
                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Balance updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating balance for user: {UserId} by amount: {Amount}", userId, amount);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "An error occurred while updating balance",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateStatsAsync(Guid userId, bool isWin, CancellationToken cancellationToken = default)
        {
            try
            {
                await _userRepository.UpdateStatsAsync(userId, isWin, cancellationToken);
                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Stats updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stats for user: {UserId}, isWin: {IsWin}", userId, isWin);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "An error occurred while updating stats",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> LinkSteamAccountAsync(Guid userId, string steamId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.UserNotFound
                    };
                }

                var existingUser = await _userRepository.GetBySteamIdAsync(steamId, cancellationToken);
                if (existingUser != null && existingUser.Id != userId)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Steam account is already linked to another user"
                    };
                }

                user.SteamId = steamId;
                await _userRepository.UpdateAsync(user, cancellationToken);
                _logger.LogInformation("Linked Steam account {SteamId} to user: {UserId}", steamId, userId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Steam account linked successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking Steam account {SteamId} to user: {UserId}", steamId, userId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "An error occurred while linking Steam account",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> LinkTelegramAccountAsync(Guid userId, string telegramUsername, long telegramChatId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.UserNotFound
                    };
                }

                user.TelegramUsername = telegramUsername;
                user.TelegramChatId = telegramChatId;
                await _userRepository.UpdateAsync(user, cancellationToken);
                _logger.LogInformation("Linked Telegram account {TelegramUsername} to user: {UserId}", telegramUsername, userId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Telegram account linked successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking Telegram account {TelegramUsername} to user: {UserId}", telegramUsername, userId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "An error occurred while linking Telegram account",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> ActivatePrimeAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.UserNotFound
                    };
                }

                user.IsPrimeActive = true;
                user.PrimeExpiresAt = DateTime.UtcNow.AddMonths(1);
                await _userRepository.UpdateAsync(user, cancellationToken);
                _logger.LogInformation("Activated Prime for user: {UserId}", userId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Prime activated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating Prime for user: {UserId}", userId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "An error occurred while activating Prime",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> DeactivatePrimeAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Code = ErrorCode.UserNotFound
                    };
                }

                user.IsPrimeActive = false;
                user.PrimeExpiresAt = null;
                await _userRepository.UpdateAsync(user, cancellationToken);
                _logger.LogInformation("Deactivated Prime for user: {UserId}", userId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Prime deactivated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating Prime for user: {UserId}", userId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "An error occurred while deactivating Prime",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<List<UserDto>>> GetTopPlayersByRatingAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                var users = await _userRepository.GetTopPlayersByRatingAsync(count, cancellationToken);
                var userDtos = _mapper.Map<List<UserDto>>(users);

                return new ApiResponse<List<UserDto>>
                {
                    Success = true,
                    Data = userDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top players by rating");
                return new ApiResponse<List<UserDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving top players",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> IsEmailAvailableAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var isAvailable = !await _userRepository.IsEmailTakenAsync(email, excludeUserId, cancellationToken);
                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = isAvailable
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email availability: {Email}", email);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while checking email availability",
                    Errors = { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> IsUsernameAvailableAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var isAvailable = !await _userRepository.IsUsernameTakenAsync(username, excludeUserId, cancellationToken);
                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = isAvailable
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking username availability: {Username}", username);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while checking username availability",
                    Errors = { ex.Message }
                };
            }
        }

    }
}
