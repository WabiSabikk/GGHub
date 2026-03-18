using GGHubApi.Configuration;
using GGHubApi.Extensions;
using GGHubApi.Hubs;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
LoggingConfiguration.ConfigureLoggingForHost(builder.Configuration);
Log.Information("Starting CS2 Duels Web API");

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddLogging(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddDatHostApiClient(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCustomAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

var app = builder.Build();

await app.Services.EnsureBotAccountAsync(app.Configuration);

app.UseForwardedHeaders();

// Додаємо глобальний middleware для обробки помилок
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TournamentHub>("/tournament-hub");
app.MapHub<DuelHub>("/duel-hub");

await app.RunAsync();
