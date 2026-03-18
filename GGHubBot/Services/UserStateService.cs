using GGHubBot.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json;

namespace GGHubBot.Services
{
    public class UserStateService
    {
        private readonly IDbContextFactory<BotDbContext> _contextFactory;

        public UserStateService(IDbContextFactory<BotDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<UserState> GetOrCreateUserStateAsync(long telegramId)
        {
            await using var context = _contextFactory.CreateDbContext();

            var userState = await context.UserStates.FirstOrDefaultAsync(u => u.TelegramId == telegramId);

            if (userState == null)
            {
                userState = new UserState { TelegramId = telegramId };
                context.UserStates.Add(userState);
                await context.SaveChangesAsync();
                Log.Information("Created new user state for Telegram ID: {TelegramId}", telegramId);
            }

            userState.LastActivity = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return userState;
        }

        public async Task UpdateUserStateAsync(long telegramId, BotState state, object? stateData = null)
        {
            await using var context = _contextFactory.CreateDbContext();

            var userState = await context.UserStates.FirstOrDefaultAsync(u => u.TelegramId == telegramId);

            if (userState == null)
            {
                userState = new UserState { TelegramId = telegramId };
                context.UserStates.Add(userState);
            }

            userState.State = state;

            userState.StateData = stateData != null ? JsonSerializer.Serialize(stateData) : null;

            userState.LastActivity = DateTime.UtcNow;

            await context.SaveChangesAsync();
        }

        public async Task<T?> GetStateDataAsync<T>(long telegramId) where T : class
        {
            await using var context = _contextFactory.CreateDbContext();

            var userState = await context.UserStates.FirstOrDefaultAsync(u => u.TelegramId == telegramId);

            if (userState == null || string.IsNullOrEmpty(userState.StateData))
                return null;

            try
            {
                return JsonSerializer.Deserialize<T>(userState.StateData);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to deserialize state data for user {TelegramId}", telegramId);
                return null;
            }
        }

        public async Task SetLanguageAsync(long telegramId, string language)
        {
            await using var context = _contextFactory.CreateDbContext();

            var userState = await context.UserStates.FirstOrDefaultAsync(u => u.TelegramId == telegramId);
            if (userState == null)
            {
                userState = new UserState { TelegramId = telegramId };
                context.UserStates.Add(userState);
            }

            userState.Language = language;
            userState.LastActivity = DateTime.UtcNow;

            await context.SaveChangesAsync();
        }

        public async Task SetAuthenticationAsync(long telegramId, Guid userId, string steamId)
        {
            await using var context = _contextFactory.CreateDbContext();

            var userState = await context.UserStates.FirstOrDefaultAsync(u => u.TelegramId == telegramId);
            if (userState == null)
            {
                userState = new UserState { TelegramId = telegramId };
                context.UserStates.Add(userState);
            }

            userState.IsAuthenticated = true;
            userState.UserId = userId;
            userState.SteamId = steamId;
            userState.LastActivity = DateTime.UtcNow;

            await context.SaveChangesAsync();
        }

        public async Task ClearStateDataAsync(long telegramId)
        {
            await using var context = _contextFactory.CreateDbContext();

            var userState = await context.UserStates.FirstOrDefaultAsync(u => u.TelegramId == telegramId);
            if (userState == null)
            {
                return;
            }

            userState.StateData = null;
            userState.LastActivity = DateTime.UtcNow;

            await context.SaveChangesAsync();
        }

        public async Task<bool> IsUserAuthenticatedAsync(long telegramId)
        {
            await using var context = _contextFactory.CreateDbContext();

            var userState = await context.UserStates.FirstOrDefaultAsync(u => u.TelegramId == telegramId);

            return userState != null &&
                   userState.IsAuthenticated &&
                   userState.UserId.HasValue &&
                   !string.IsNullOrEmpty(userState.SteamId);
        }

        public async Task CleanupOldStatesAsync()
        {
            await using var context = _contextFactory.CreateDbContext();

            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            var oldStates = await context.UserStates
                .Where(u => u.LastActivity < cutoffDate && !u.IsAuthenticated)
                .ToListAsync();

            if (oldStates.Any())
            {
                context.UserStates.RemoveRange(oldStates);
                await context.SaveChangesAsync();
                Log.Information("Cleaned up {Count} old user states", oldStates.Count);
            }
        }

    }
}
