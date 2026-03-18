using Serilog;
using Serilog.Events;
using Serilog.Filters;

namespace GGHubApi.Configuration
{
    public static class LoggingConfiguration
    {
        public static IServiceCollection AddLogging(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Logger = CreateLogger(configuration);

            services.AddSerilog();

            return services;
        }

        private static Serilog.ILogger CreateLogger(IConfiguration configuration)
        {
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "CS2Duels.Web.Api")
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    path: Path.Combine("logs", "cs2duels-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.Logger(errorLogger => errorLogger
                    .Filter.ByIncludingOnly(Matching.WithProperty<LogEventLevel>("Level", p => p >= LogEventLevel.Error))
                    .WriteTo.File(
                        path: Path.Combine("logs", "errors", "error-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 90,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}"));

           

            return loggerConfig.CreateLogger();
        }

        public static void ConfigureLoggingForHost(IConfiguration configuration)
        {
            Log.Logger = CreateLogger(configuration);
        }
    }
    }
