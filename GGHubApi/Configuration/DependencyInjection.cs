using GGHubApi.Hubs;
using GGHubApi.MapProfiles;
using GGHubApi.Services;
using GGHubDb;
using GGHubDb.Repos;
using GGHubDb.Services;
using Microsoft.EntityFrameworkCore;

namespace GGHubApi.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabase(configuration);
            services.AddRepositories();
            services.AddBusinessServices();
            services.AddExternalServices();
            services.AddTournamentServices();
            services.AddMapping();
            services.AddSignalR();

            return services;
        }

        private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(120);
                });

                options.EnableSensitiveDataLogging(false);
                options.EnableDetailedErrors(false);
                options.LogTo(Console.WriteLine, LogLevel.Warning);
            });

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IDuelRepository, DuelRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();

            services.AddScoped<IMatchConfigRepository, MatchConfigRepository>();
            services.AddScoped<IServerRepository, ServerRegionRepository>();

            // Tournament repositories
            services.AddScoped<ITournamentRepository, TournamentRepository>();
            services.AddScoped<ITournamentTeamRepository, TournamentTeamRepository>();
            services.AddScoped<ITournamentMatchRepository, TournamentMatchRepository>();
            services.AddScoped<ITournamentPaymentRepository, TournamentPaymentRepository>();

            return services;
        }

        private static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IDuelService, DuelService>();
            services.AddScoped<IPingAnalysisService, PingAnalysisService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<ITournamentService, TournamentService>();
            services.AddScoped<IMatchMetricsService, MatchMetricsService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IMatchConfigService, MatchConfigService>();
            services.AddScoped<IServerService, ServerService>();
            services.AddScoped<IDathostWebhookService, DathostWebhookService>();

            return services;
        }

        private static IServiceCollection AddExternalServices(this IServiceCollection services)
        {
            services.AddHttpClient("Cryptomus", client =>
            {
                client.BaseAddress = new Uri("https://api.cryptomus.com/v1/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddHttpClient("Dathost", client =>
            {
                client.BaseAddress = new Uri("https://dathost.net/api/");
                client.Timeout = TimeSpan.FromSeconds(60);
            });

            return services;
        }

        private static IServiceCollection AddTournamentServices(this IServiceCollection services)
        {
            services.AddScoped<IDathostService, DathostService>();
            services.AddScoped<ICryptomusService, CryptomusService>();
            services.AddScoped<ITournamentHubService, TournamentHubService>();
            services.AddScoped<IDuelHubService, DuelHubService>();
            services.AddScoped<IUserMappingService, UserMappingService>();
            services.AddSingleton<IConnectionManager, ConnectionManager>();

            return services;
        }

        private static IServiceCollection AddMapping(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfiles));
            return services;
        }
    }
}