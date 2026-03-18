using GGHubApi.Extensions;
using GGHubShared.Models;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;
using System.Text.Json;

namespace GGHubApi.Configuration
{
    public static class SteamAuthConfiguration
    {
        public static IServiceCollection AddSteamAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication()
                .AddSteam(options =>
                {
                    options.ApplicationKey = configuration["Steam:ApiKey"]!;
                    options.CallbackPath = "/auth/steam/callback";
                    options.ReturnUrlParameter = "returnUrl";

                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                        {
                            // Extract Steam ID from claims
                            var steamIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier);
                            if (steamIdClaim != null)
                            {
                                // Get additional Steam user info
                                await GetSteamUserInfo(context, steamIdClaim.Value, configuration["Steam:ApiKey"]!);
                            }
                        },
                        OnRemoteFailure = context =>
                        {
                            context.Response.Redirect("/auth/steam/error");
                            context.HandleResponse();
                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }

        private static async Task GetSteamUserInfo(OAuthCreatingTicketContext context, string steamId, string apiKey)
        {
            try
            {
                using var httpClient = new HttpClient();
                var url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={apiKey}&steamids={steamId}";

                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var steamResponse = JsonSerializer.Deserialize<SteamUserResponse>(content);

                    if (steamResponse?.Response?.Players?.Any() == true)
                    {
                        var player = steamResponse.Response.Players.First();

                        // Add additional claims
                        var identity = (ClaimsIdentity)context.Principal!.Identity!;
                        identity.AddClaim(new Claim("steam_persona_name", player.PersonaName ?? ""));
                        identity.AddClaim(new Claim("steam_avatar", player.Avatar ?? ""));
                        identity.AddClaim(new Claim("steam_avatar_medium", player.AvatarMedium ?? ""));
                        identity.AddClaim(new Claim("steam_avatar_full", player.AvatarFull ?? ""));
                        identity.AddClaim(new Claim("steam_profile_url", player.ProfileUrl ?? ""));
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail authentication
                Console.WriteLine($"Error getting Steam user info: {ex.Message}");
            }
        }
    }
}
