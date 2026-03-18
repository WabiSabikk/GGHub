using DatHost.Api.Client;
using DatHostApi;
using Microsoft.Extensions.Options;
using static DatHost.Api.Client.IDatHostApiClient;

namespace GGHubApi.Extensions;

/// <summary>
/// Extension methods for configuring DatHost API client in dependency injection
/// </summary>
public static class DatHostServices
{
    /// <summary>
    /// Add DatHost API client services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="sectionName">Configuration section name (default: "DatHost")</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDatHostApiClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "DatHost")
    {
        // Configure options
        services.Configure<DatHostApiOptions>(configuration.GetSection(sectionName));

        // Add HttpClient with retry policy for high-performance scenarios
        services.AddHttpClient<IDatHostApiClient, DatHostApiClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<DatHostApiOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

            // Configure connection pooling settings for high load
            client.DefaultRequestHeaders.Connection.Add("keep-alive");
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            // Optimize for high-performance scenarios
            MaxConnectionsPerServer = Environment.ProcessorCount * 4,
            UseCookies = false,
            UseProxy = false
        });

        // Register services
        services.AddScoped<IGameServerService, GameServerService>();
        services.AddScoped<ICs2MatchService, Cs2MatchService>();
        services.AddScoped<ICustomDomainService, CustomDomainService>();

        return services;
    }

    /// <summary>
    /// Add DatHost API client services with custom options
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Options configuration action</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDatHostApiClient(
        this IServiceCollection services,
        Action<DatHostApiOptions> configureOptions)
    {
        // Configure options
        services.Configure(configureOptions);

        // Add HttpClient with retry policy for high-performance scenarios
        services.AddHttpClient<IDatHostApiClient, DatHostApiClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<DatHostApiOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

            // Configure connection pooling settings for high load
            client.DefaultRequestHeaders.Connection.Add("keep-alive");
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            // Optimize for high-performance scenarios
            MaxConnectionsPerServer = Environment.ProcessorCount * 4,
            UseCookies = false,
            UseProxy = false
        });

        // Register services
        services.AddScoped<IGameServerService, GameServerService>();
        services.AddScoped<ICs2MatchService, Cs2MatchService>();
        services.AddScoped<ICustomDomainService, CustomDomainService>();

        return services;
    }

    /// <summary>
    /// Add DatHost API client services with manual options
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="options">API client options</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDatHostApiClient(
        this IServiceCollection services,
        DatHostApiOptions options)
    {
        // Configure options
        services.Configure<DatHostApiOptions>(opt =>
        {
            opt.BaseUrl = options.BaseUrl;
            opt.Email = options.Email;
            opt.Password = options.Password;
            opt.AccountEmail = options.AccountEmail;
            opt.TimeoutSeconds = options.TimeoutSeconds;
            opt.MaxRetryAttempts = options.MaxRetryAttempts;
            opt.RetryDelayMs = options.RetryDelayMs;
        });

        // Add HttpClient with retry policy for high-performance scenarios
        services.AddHttpClient<IDatHostApiClient, DatHostApiClient>((serviceProvider, client) =>
        {
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

            // Configure connection pooling settings for high load
            client.DefaultRequestHeaders.Connection.Add("keep-alive");
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            // Optimize for high-performance scenarios
            MaxConnectionsPerServer = Environment.ProcessorCount * 4,
            UseCookies = false,
            UseProxy = false
        });

        // Register services
        services.AddScoped<IGameServerService, GameServerService>();
        services.AddScoped<ICs2MatchService, Cs2MatchService>();
        services.AddScoped<ICustomDomainService, CustomDomainService>();

        return services;
    }
}

