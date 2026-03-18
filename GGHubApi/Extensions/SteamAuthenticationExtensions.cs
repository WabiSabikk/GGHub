using GGHubApi.Services;
using Microsoft.AspNetCore.Authentication;

namespace GGHubApi.Extensions
{
    public static class SteamAuthenticationExtensions
    {
        public static AuthenticationBuilder AddSteam(this AuthenticationBuilder builder, Action<SteamAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<SteamAuthenticationOptions, SteamAuthenticationHandler>("Steam", configureOptions);
        }
    }
}
