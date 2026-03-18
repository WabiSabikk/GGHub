using GGHubBot;
using GGHubBot.Handlers;
using GGHubBot.Services;
using GGHubBot.Features.Common;
using GGHubBot.Features.Duels;
using GGHubBot.Features.Payments;
using GGHubBot.Features.Main;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Telegram.Bot;
using GGHubBot.Models;

var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/bot-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContextFactory<BotDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<ITelegramBotClient>(provider =>
    new TelegramBotClient(builder.Configuration["TelegramBot:Token"]!));

builder.Services.AddSingleton<TelegramBotService>();
builder.Services.AddSingleton<UserStateService>();
builder.Services.AddSingleton<LocalizationService>();
builder.Services.AddSingleton<AdminService>();
builder.Services.AddSingleton<MediaService>();

builder.Services.AddSingleton<MessageHandler>();
builder.Services.AddSingleton<CallbackHandler>();
builder.Services.AddSingleton<TournamentHandler>();

// Feature services
builder.Services.AddSingleton<DuelService>();
builder.Services.AddSingleton<DuelUIService>();
builder.Services.AddSingleton<PaymentService>();
builder.Services.AddSingleton<MenuUIService>();
builder.Services.AddSingleton<PaymentUIService>();

builder.Services.AddHostedService<DuelHubClientService>();

// Command handlers
builder.Services.AddSingleton<ICallbackCommand, DuelCallbackHandler>();
builder.Services.AddSingleton<ICallbackCommand, PaymentCallbackHandler>();

// Command resolver
builder.Services.AddSingleton<CallbackCommandResolver>();

builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["GGHubApi:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHostedService<TelegramBotHostedService>();

var app = builder.Build();

Log.Information("CS2 Duels Telegram Bot started");

app.MapGet("/steam/auth", ([FromQuery] long chatId, HttpResponse response) =>
{
    var callbackUrl = builder.Configuration["Steam:RedirectUrl"] ?? string.Empty;
    var apiBase = builder.Configuration["GGHubApi:BaseUrl"] ?? string.Empty;
    var apiRoot = apiBase.EndsWith("/api/") ? apiBase[..^4] : apiBase;
    var returnUrl = $"{callbackUrl}?chatId={chatId}";
    var loginUrl = $"{apiRoot}auth/steam/login?returnUrl={Uri.EscapeDataString(returnUrl)}";
    response.Redirect(loginUrl);
    return Results.Empty;
});

app.MapGet("/steam/callback", async ([FromQuery] long chatId, [FromQuery] string token,
    [FromServices] ApiService apiService,
    [FromServices] UserStateService userStateService,
    [FromServices] MenuUIService menuUIService) =>
{
    var handler = new JwtSecurityTokenHandler();
    var jwt = handler.ReadJwtToken(token);
    var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub || c.Type == ClaimTypes.NameIdentifier)?.Value;
    if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        return Results.BadRequest("Invalid token");

    var userResponse = await apiService.GetUserByIdAsync(userId);
    await apiService.LinkTelegramAccountAsync(userId, null, chatId);
    await userStateService.SetAuthenticationAsync(chatId, userId, userResponse.Data.SteamId);

    var userState = await userStateService.GetOrCreateUserStateAsync(chatId);
    await menuUIService.ShowMainMenuAsync(chatId, userState.Language, CancellationToken.None);
    await userStateService.UpdateUserStateAsync(chatId, BotState.MainMenu);

    return Results.Text("Authentication successful. You can return to Telegram.");
});

app.Run();