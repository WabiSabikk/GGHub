using GGHubApi.Configuration;
using GGHubApi.Services;
using GGHubDb.Models;
using GGHubDb.Repos;
using GGHubShared.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;
using System.Text.Json;

namespace GGHubApi.Extensions;

public static class ApiExtensions
{
    public static async Task EnsureBotAccountAsync(this IServiceProvider services, IConfiguration configuration)
    {
        var email = configuration["BotAccount:Email"];
        var password = configuration["BotAccount:Password"];
        var username = configuration["BotAccount:Username"] ?? "Bot";
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        using var scope = services.CreateScope();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var existing = await userRepo.GetByEmailAsync(email);
        if (existing != null)
            return;

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHasher.HashPassword(password),
            Role = UserRole.Bot
        };

        await userRepo.AddAsync(user);
    }
}

public static class AuthenticationExtensions
{
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        services.AddAuthentication()
            .AddSteam(options =>
            {
                options.ApplicationKey = configuration["Steam:ApiKey"] ?? string.Empty;
                options.CallbackPath = "/auth/steam/callback";
                options.ReturnUrlParameter = "returnUrl";
            });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()!;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
            };
        });

        return services;
    }
}

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new { success = false, message = "Internal server error" });
            await context.Response.WriteAsync(result);
        }
    }
}

