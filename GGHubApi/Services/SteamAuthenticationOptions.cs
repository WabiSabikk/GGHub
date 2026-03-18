using Azure;
using Azure.Core;
using GGHubShared.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace GGHubApi.Services
{
    public class SteamAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string ApplicationKey { get; set; } = string.Empty;
        public string CallbackPath { get; set; } = "/auth/steam/callback";
        public string ReturnUrlParameter { get; set; } = "returnUrl";
    }

    public class SteamAuthenticationHandler : AuthenticationHandler<SteamAuthenticationOptions>
    {
        private const string SteamOpenIdUrl = "https://steamcommunity.com/openid";
        private readonly ILogger<SteamAuthenticationHandler> _logger;

        public SteamAuthenticationHandler(IOptionsMonitor<SteamAuthenticationOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _logger = logger.CreateLogger<SteamAuthenticationHandler>();
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                // Check if this is a callback from Steam
                if (!Request.Path.Equals(Options.CallbackPath, StringComparison.OrdinalIgnoreCase))
                {
                    return AuthenticateResult.NoResult();
                }

                // Validate OpenID response
                if (!await ValidateOpenIdResponseAsync())
                {
                    return AuthenticateResult.Fail("Invalid OpenID response from Steam");
                }

                // Extract Steam ID from the response
                var steamId = ExtractSteamIdFromResponse();
                if (string.IsNullOrEmpty(steamId))
                {
                    return AuthenticateResult.Fail("Could not extract Steam ID from response");
                }

                // Create claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, steamId),
                    new Claim("steam_id", steamId),
                    new Claim(ClaimTypes.AuthenticationMethod, "steam")
                };

                // Get additional Steam user info if API key is available
                if (!string.IsNullOrEmpty(Options.ApplicationKey))
                {
                    await AddSteamUserInfoClaims(claims, steamId);
                }

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Steam authentication");
                return AuthenticateResult.Fail(ex.Message);
            }
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            var returnUrl = properties.RedirectUri ?? $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Options.CallbackPath}";
            var steamOpenIdUrl = BuildSteamOpenIdUrl(returnUrl);

            Response.Redirect(steamOpenIdUrl);
            return Task.CompletedTask;
        }

        private string BuildSteamOpenIdUrl(string returnUrl)
        {
            var parameters = new Dictionary<string, string>
            {
                ["openid.ns"] = "http://specs.openid.net/auth/2.0",
                ["openid.mode"] = "checkid_setup",
                ["openid.return_to"] = returnUrl,
                ["openid.realm"] = $"{Request.Scheme}://{Request.Host}",
                ["openid.identity"] = "http://specs.openid.net/auth/2.0/identifier_select",
                ["openid.claimed_id"] = "http://specs.openid.net/auth/2.0/identifier_select"
            };

            var queryString = string.Join("&", parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
            return $"{SteamOpenIdUrl}/login?{queryString}";
        }

        private async Task<bool> ValidateOpenIdResponseAsync()
        {
            IDictionary<string, string> validationParams;

            if (HttpMethods.IsPost(Request.Method) && Request.HasFormContentType)
            {
                var form = await Request.ReadFormAsync();

                if (!form.TryGetValue("openid.mode", out var mode) || mode != "id_res")
                {
                    return false;
                }

                validationParams = form.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
            }
            else
            {
                var query = Request.Query;

                if (!query.TryGetValue("openid.mode", out var mode) || mode != "id_res")
                {
                    return false;
                }

                validationParams = query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
            }

            validationParams["openid.mode"] = "check_authentication";

            using var httpClient = new HttpClient();
            var content = new FormUrlEncodedContent(validationParams);
            var response = await httpClient.PostAsync($"{SteamOpenIdUrl}/login", content);
            var responseText = await response.Content.ReadAsStringAsync();

            return responseText.Contains("is_valid:true");
        }

        private string? ExtractSteamIdFromResponse()
        {
            var claimedId = Request.Query["openid.claimed_id"].FirstOrDefault();
            if (string.IsNullOrEmpty(claimedId))
                return null;

            var match = Regex.Match(claimedId, @"https://steamcommunity\.com/openid/id/(\d+)");
            return match.Success ? match.Groups[1].Value : null;
        }

        private async Task AddSteamUserInfoClaims(List<Claim> claims, string steamId)
        {
            try
            {
                using var httpClient = new HttpClient();
                var url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={Options.ApplicationKey}&steamids={steamId}";

                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var steamResponse = System.Text.Json.JsonSerializer.Deserialize<SteamUserResponse>(content);

                    if (steamResponse?.Response?.Players?.Any() == true)
                    {
                        var player = steamResponse.Response.Players.First();

                        claims.Add(new Claim(ClaimTypes.Name, player.PersonaName ?? ""));
                        claims.Add(new Claim("steam_persona_name", player.PersonaName ?? ""));
                        claims.Add(new Claim("steam_avatar", player.Avatar ?? ""));
                        claims.Add(new Claim("steam_avatar_medium", player.AvatarMedium ?? ""));
                        claims.Add(new Claim("steam_avatar_full", player.AvatarFull ?? ""));
                        claims.Add(new Claim("steam_profile_url", player.ProfileUrl ?? ""));
                        claims.Add(new Claim("occountrycode", player.LocationCountryCode ?? ""));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not fetch Steam user info for Steam ID: {SteamId}", steamId);
            }
        }
    }

}
