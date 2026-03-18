using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GGHubBot.Services
{
    public class AdminService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IDbContextFactory<BotDbContext> _contextFactory;
        private readonly ApiService _apiService;
        private readonly LocalizationService _localizationService;
        private readonly IConfiguration _configuration;
        private readonly long[] _adminIds;

        public AdminService(
            ITelegramBotClient botClient,
            IDbContextFactory<BotDbContext> contextFactory,
            ApiService apiService,
            LocalizationService localizationService,
            IConfiguration configuration)
        {
            _botClient = botClient;
            _contextFactory = contextFactory;
            _apiService = apiService;
            _localizationService = localizationService;
            _configuration = configuration;
            _adminIds = configuration.GetSection("TelegramBot:AdminIds").Get<long[]>() ?? Array.Empty<long>();
        }

        public bool IsAdmin(long userId)
        {
            return _adminIds.Contains(userId);
        }

        public async Task HandleAdminCommandAsync(Message message, CancellationToken cancellationToken)
        {
            if (!IsAdmin(message.From!.Id))
                return;

            var command = message.Text?.ToLower();

            switch (command)
            {
                case "/admin":
                    await ShowAdminMenuAsync(message.Chat.Id, cancellationToken);
                    break;

                case "/stats":
                    await ShowStatsAsync(message.Chat.Id, cancellationToken);
                    break;

                case "/users":
                    await ShowUsersStatsAsync(message.Chat.Id, cancellationToken);
                    break;

                case "/broadcast":
                    await _botClient.SendMessage(
                        message.Chat.Id,
                        "📢 Send the message you want to broadcast to all users:",
                        cancellationToken: cancellationToken);
                    break;

                case var msg when msg.StartsWith("/ban "):
                    await HandleBanUserAsync(message, cancellationToken);
                    break;

                case var msg when msg.StartsWith("/unban "):
                    await HandleUnbanUserAsync(message, cancellationToken);
                    break;

                default:
                    if (command?.StartsWith("/") == true)
                    {
                        await _botClient.SendMessage(
                            message.Chat.Id,
                            "❌ Unknown admin command",
                            cancellationToken: cancellationToken);
                    }
                    break;
            }
        }

        public async Task HandleCallbackAsync(CallbackQuery callbackQuery, string data, CancellationToken cancellationToken)
        {
            if (!IsAdmin(callbackQuery.From.Id))
                return;

            var chatId = callbackQuery.Message!.Chat.Id;

            switch (data)
            {
                case "admin_stats":
                    await ShowStatsAsync(chatId, cancellationToken);
                    break;

                case "admin_users":
                    await ShowUsersStatsAsync(chatId, cancellationToken);
                    break;

                case "admin_duels":
                    await ShowDuelsStatsAsync(chatId, cancellationToken);
                    break;

                case "admin_tournaments":
                    await ShowTournamentsStatsAsync(chatId, cancellationToken);
                    break;

                case "admin_cleanup":
                    await PerformCleanupAsync(chatId, cancellationToken);
                    break;

                case "admin_broadcast":
                    await _botClient.SendMessage(
                        chatId,
                        "📢 Send the message you want to broadcast to all users:",
                        cancellationToken: cancellationToken);
                    break;

                case "admin_logs":
                    await ShowRecentLogsAsync(chatId, cancellationToken);
                    break;
            }

            await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
        }

        private async Task ShowAdminMenuAsync(long chatId, CancellationToken cancellationToken)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📊 Statistics", "admin_stats"),
                InlineKeyboardButton.WithCallbackData("👥 Users", "admin_users")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("⚔️ Duels", "admin_duels"),
                InlineKeyboardButton.WithCallbackData("🏆 Tournaments", "admin_tournaments")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📢 Broadcast", "admin_broadcast"),
                InlineKeyboardButton.WithCallbackData("🧹 Cleanup", "admin_cleanup")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📋 Logs", "admin_logs")
            }
        });

            await _botClient.SendMessage(
                chatId,
                "🔧 Admin Panel\n\nSelect an option:",
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }

        private async Task ShowStatsAsync(long chatId, CancellationToken cancellationToken)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();

                var totalUsers = await context.UserStates.CountAsync(cancellationToken);
                var authenticatedUsers = await context.UserStates.CountAsync(u => u.IsAuthenticated, cancellationToken);
                var todayUsers = await context.UserStates.CountAsync(u => u.LastActivity >= DateTime.UtcNow.Date, cancellationToken);
                var weekUsers = await context.UserStates.CountAsync(u => u.LastActivity >= DateTime.UtcNow.AddDays(-7), cancellationToken);

                var languageStats = await context.UserStates
                    .GroupBy(u => u.Language)
                    .Select(g => new { Language = g.Key, Count = g.Count() })
                    .ToListAsync(cancellationToken);

                var text = "📊 Bot Statistics\n\n" +
                          $"👥 Total users: {totalUsers}\n" +
                          $"🔐 Authenticated: {authenticatedUsers}\n" +
                          $"📅 Active today: {todayUsers}\n" +
                          $"📅 Active this week: {weekUsers}\n\n" +
                          "🌍 Languages:\n";

                foreach (var stat in languageStats)
                {
                    var langName = _localizationService.GetLanguageName(stat.Language);
                    text += $"  {langName}: {stat.Count}\n";
                }

                await _botClient.SendMessage(
                    chatId,
                    text,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error showing admin stats");
                await _botClient.SendMessage(
                    chatId,
                    "❌ Error retrieving statistics",
                    cancellationToken: cancellationToken);
            }
        }

        private async Task ShowUsersStatsAsync(long chatId, CancellationToken cancellationToken)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();

                var recentUsers = await context.UserStates
                    .Where(u => u.IsAuthenticated)
                    .OrderByDescending(u => u.LastActivity)
                    .Take(10)
                    .Select(u => new
                    {
                        u.TelegramId,
                        u.LastActivity,
                        u.Language,
                        u.State
                    })
                    .ToListAsync(cancellationToken);

                var text = "👥 Recent Authenticated Users\n\n";

                foreach (var user in recentUsers)
                {
                    var timeDiff = DateTime.UtcNow - user.LastActivity;
                    var timeStr = timeDiff.TotalHours < 1 ?
                        $"{(int)timeDiff.TotalMinutes}m ago" :
                        $"{(int)timeDiff.TotalHours}h ago";

                    text += $"🔸 ID: {user.TelegramId}\n";
                    text += $"   Last: {timeStr} | {user.Language} | {user.State}\n\n";
                }

                await _botClient.SendMessage(
                    chatId,
                    text,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error showing users stats");
                await _botClient.SendMessage(
                    chatId,
                    "❌ Error retrieving user statistics",
                    cancellationToken: cancellationToken);
            }
        }

        private async Task ShowDuelsStatsAsync(long chatId, CancellationToken cancellationToken)
        {
            try
            {
                var metrics = await _apiService.GetGlobalMatchMetricsAsync();
                if (metrics?.Success == true && metrics.Data != null)
                {
                    var text = "⚔️ Duels Statistics\n\n" +
                               $"Total matches: {metrics.Data.TotalMatches}\n" +
                               $"Average deposit: €{metrics.Data.AverageDeposit:F2}";
                    await _botClient.SendMessage(chatId, text, cancellationToken: cancellationToken);
                }
                else
                {
                    await _botClient.SendMessage(chatId, "❌ Failed to load metrics", cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error showing duel metrics");
                await _botClient.SendMessage(chatId, "❌ Error retrieving metrics", cancellationToken: cancellationToken);
            }
        }

        private async Task ShowTournamentsStatsAsync(long chatId, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(
                chatId,
                "🏆 Tournaments Statistics\n\n⏳ Loading from API...",
                cancellationToken: cancellationToken);
        }

        private async Task PerformCleanupAsync(long chatId, CancellationToken cancellationToken)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();

                var cutoffDate = DateTime.UtcNow.AddDays(-30);
                var oldStates = await context.UserStates
                    .Where(u => u.LastActivity < cutoffDate && !u.IsAuthenticated)
                    .ToListAsync(cancellationToken);

                if (oldStates.Any())
                {
                    context.UserStates.RemoveRange(oldStates);
                    await context.SaveChangesAsync(cancellationToken);

                    await _botClient.SendMessage(
                        chatId,
                        $"🧹 Cleanup completed!\n\nRemoved {oldStates.Count} old user states",
                        cancellationToken: cancellationToken);

                    Log.Information("Admin cleanup: removed {Count} old user states", oldStates.Count);
                }
                else
                {
                    await _botClient.SendMessage(
                        chatId,
                        "🧹 No cleanup needed - database is clean!",
                        cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during admin cleanup");
                await _botClient.SendMessage(
                    chatId,
                    "❌ Error during cleanup operation",
                    cancellationToken: cancellationToken);
            }
        }

        private async Task HandleBanUserAsync(Message message, CancellationToken cancellationToken)
        {
            var parts = message.Text!.Split(' ');
            if (parts.Length < 2 || !long.TryParse(parts[1], out var userId))
            {
                await _botClient.SendMessage(
                    message.Chat.Id,
                    "❌ Usage: /ban <user_id>",
                    cancellationToken: cancellationToken);
                return;
            }

            await _botClient.SendMessage(
                message.Chat.Id,
                $"🚫 User {userId} would be banned (not implemented)",
                cancellationToken: cancellationToken);

            Log.Warning("Admin {AdminId} attempted to ban user {UserId}", message.From!.Id, userId);
        }

        private async Task HandleUnbanUserAsync(Message message, CancellationToken cancellationToken)
        {
            var parts = message.Text!.Split(' ');
            if (parts.Length < 2 || !long.TryParse(parts[1], out var userId))
            {
                await _botClient.SendMessage(
                    message.Chat.Id,
                    "❌ Usage: /unban <user_id>",
                    cancellationToken: cancellationToken);
                return;
            }

            await _botClient.SendMessage(
                message.Chat.Id,
                $"✅ User {userId} would be unbanned (not implemented)",
                cancellationToken: cancellationToken);

            Log.Information("Admin {AdminId} attempted to unban user {UserId}", message.From!.Id, userId);
        }

        private async Task ShowRecentLogsAsync(long chatId, CancellationToken cancellationToken)
        {
            try
            {
                var logPath = Path.Combine("logs", $"bot-{DateTime.UtcNow:yyyyMMdd}.log");

                if (File.Exists(logPath))
                {
                    var lines = await File.ReadAllLinesAsync(logPath, cancellationToken);
                    var recentLines = lines.TakeLast(20).ToArray();

                    var text = "📋 Recent Logs (last 20 lines)\n\n```\n" +
                              string.Join("\n", recentLines) +
                              "\n```";

                    await _botClient.SendMessage(
                        chatId,
                        text,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                }
                else
                {
                    await _botClient.SendMessage(
                        chatId,
                        "📋 No log file found for today",
                        cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error showing recent logs");
                await _botClient.SendMessage(
                    chatId,
                    "❌ Error reading log files",
                    cancellationToken: cancellationToken);
            }
        }

        public async Task BroadcastMessageAsync(string message, CancellationToken cancellationToken)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();

                var users = await context.UserStates
                    .Where(u => u.IsAuthenticated)
                    .Select(u => u.TelegramId)
                    .ToListAsync(cancellationToken);

                var successCount = 0;
                var failCount = 0;

                foreach (var userId in users)
                {
                    try
                    {
                        await _botClient.SendMessage(
                            userId,
                            $"📢 Broadcast Message\n\n{message}",
                            cancellationToken: cancellationToken);

                        successCount++;
                        await Task.Delay(100, cancellationToken);
                    }
                    catch
                    {
                        failCount++;
                    }
                }

                Log.Information("Broadcast completed: {Success} successful, {Failed} failed", successCount, failCount);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during broadcast");
            }
        }

        public async Task NotifyAdminsAsync(string message, CancellationToken cancellationToken = default)
        {
            foreach (var adminId in _adminIds)
            {
                try
                {
                    await _botClient.SendMessage(
                        adminId,
                        $"🔔 Admin Notification\n\n{message}",
                        cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to notify admin {AdminId}", adminId);
                }
            }
        }
    }
}
