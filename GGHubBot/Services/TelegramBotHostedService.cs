using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace GGHubBot.Services
{
    public class TelegramBotHostedService : BackgroundService
    {
        private readonly TelegramBotService _botService;
        private readonly ApiService _apiService;
        private readonly IDbContextFactory<BotDbContext> _contextFactory;

        public TelegramBotHostedService(
            TelegramBotService botService,
            ApiService apiService,
            IDbContextFactory<BotDbContext> contextFactory)
        {
            _botService = botService;
            _apiService = apiService;
            _contextFactory = contextFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(stoppingToken);
            await context.Database.EnsureCreatedAsync(stoppingToken);

            await _apiService.InitializeAsync();
            await _botService.StartAsync();

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // ignore cancellation
            }
        }
    }
}
