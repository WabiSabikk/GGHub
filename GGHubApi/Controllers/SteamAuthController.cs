using GGHubApi.Services;
using GGHubDb.Services;
using GGHubShared.Enums;
using GGHubShared.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GGHubApi.Controllers
{
    [ApiController]
    [Route("auth/steam")]
    [AllowAnonymous]
    public class SteamAuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<SteamAuthController> _logger;

        public SteamAuthController(
            IUserService userService,
            IJwtTokenService jwtTokenService,
            IPasswordHasher passwordHasher,
            ILogger<SteamAuthController> logger)
        {
            _userService = userService;
            _jwtTokenService = jwtTokenService;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        /// <summary>
        /// Initiates Steam OAuth login process
        /// </summary>
        [HttpGet("login")]
        public IActionResult Login([FromQuery] string? returnUrl = null)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(
                    nameof(Callback),
                    "SteamAuth",
                    new { returnUrl },
                    Request.Scheme,
                    Request.Host.ToString())
            };

            return Challenge(properties, "Steam");
        }

        /// <summary>
        /// Handles Steam openID callback
        /// </summary>
        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var authenticateResult = await HttpContext.AuthenticateAsync("Steam");

                if (!authenticateResult.Succeeded)
                {
                    _logger.LogWarning("Steam authentication failed: {Error}", authenticateResult.Failure?.Message);
                    return BadRequest(new ApiResponse<AuthResponse>
                    {
                        Success = false,
                        Code = ErrorCode.Unauthorized
                    });
                }

                var steamId = authenticateResult.Principal?.FindFirst("steam_id")?.Value;
                var personaName = authenticateResult.Principal?.FindFirst("steam_persona_name")?.Value;
                var avatar = authenticateResult.Principal?.FindFirst("steam_avatar")?.Value;
                var country = authenticateResult.Principal?.FindFirst("loccountrycode")?.Value;

                if (string.IsNullOrEmpty(steamId))
                {
                    return BadRequest(new ApiResponse<AuthResponse>
                    {
                        Success = false,
                        Code = ErrorCode.ValidationFailed
                    });
                }

                _logger.LogInformation("Steam authentication successful for Steam ID: {SteamId}", steamId);

                // Try to find existing user by Steam ID
                var existingUserResult = await _userService.GetByEmailAsync($"steam_{steamId}@temp.local", cancellationToken);

                UserDto user;
                bool isNewUser = false;

                if (existingUserResult.Success && existingUserResult.Data != null)
                {
                    // Existing user - update Steam info if needed
                    user = existingUserResult.Data;
                    await _userService.LinkSteamAccountAsync(user.Id, steamId, cancellationToken);
                }
                else
                {
                    // New user - create account
                    var username = personaName ?? $"SteamUser_{steamId[^6..]}";
                    var email = $"steam_{steamId}@temp.local"; // Temporary email for Steam users

                    var createUserResult = await _userService.CreateUserAsync(username, email, avatar, country, cancellationToken: cancellationToken);

                    if (!createUserResult.Success || createUserResult.Data == null)
                    {
                        return BadRequest(new ApiResponse<AuthResponse>
                        {
                            Success = false,
                            Code = createUserResult.Code
                        });
                    }

                    user = createUserResult.Data;
                    isNewUser = true;

                    // Link Steam account
                    await _userService.LinkSteamAccountAsync(user.Id, steamId, cancellationToken);

                    _logger.LogInformation("Created new user account for Steam ID: {SteamId}, User ID: {UserId}", steamId, user.Id);
                }

                // Generate JWT token
                var token = _jwtTokenService.GenerateToken(user.Id, user.Role.ToString());

                var authResponse = new AuthResponse
                {
                    Token = token,
                    User = user
                };

                // Return response based on client type
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    var decodedUrl = Uri.UnescapeDataString(returnUrl);
                    var separator = decodedUrl.Contains('?') ? '&' : '?';
                    var redirectUrl = $"{decodedUrl}{separator}token={token}&isNewUser={isNewUser}";
                    return Redirect(redirectUrl);
                }

                // For API client - return JSON
                return Ok(new ApiResponse<AuthResponse>
                {
                    Success = true,
                    Data = authResponse,
                    Message = isNewUser ? "Account created and logged in successfully" : "Logged in successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Steam authentication callback");
                return StatusCode(500, new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Code = ErrorCode.ServerError
                });
            }
        }

        /// <summary>
        /// Links Steam account to existing user account
        /// </summary>
        [HttpPost("link")]
        [Authorize]
        public async Task<IActionResult> LinkSteamAccount([FromBody] LinkSteamAccountRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new ApiResponse<bool>
                    {
                        Success = false,
                        Code = ErrorCode.Unauthorized
                    });
                }

                var result = await _userService.LinkSteamAccountAsync(userId, request.SteamId, cancellationToken);

                if (!result.Success)
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Code = result.Code
                    });
                }

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Steam account linked successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking Steam account");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Code = ErrorCode.ServerError
                });
            }
        }

        /// <summary>
        /// Gets Steam user info for verification
        /// </summary>
        [HttpGet("userinfo/{steamId}")]
        public async Task<IActionResult> GetSteamUserInfo(string steamId, CancellationToken cancellationToken = default)
        {
            try
            {
                var apiKey = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Steam:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    return BadRequest(new ApiResponse<SteamUserInfo>
                    {
                        Success = false,
                        Code = ErrorCode.ServerError
                    });
                }

                using var httpClient = new HttpClient();
                var url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={apiKey}&steamids={steamId}";

                var response = await httpClient.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest(new ApiResponse<SteamUserInfo>
                    {
                        Success = false,
                        Code = ErrorCode.ServerError
                    });
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var steamResponse = System.Text.Json.JsonSerializer.Deserialize<SteamUserResponse>(content);

                if (steamResponse?.Response?.Players?.Any() != true)
                {
                    return NotFound(new ApiResponse<SteamUserInfo>
                    {
                        Success = false,
                        Code = ErrorCode.NotFound
                    });
                }

                var player = steamResponse.Response.Players.First();
                var userInfo = new SteamUserInfo
                {
                    SteamId = player.SteamId ?? steamId,
                    PersonaName = player.PersonaName ?? "",
                    ProfileUrl = player.ProfileUrl ?? "",
                    Avatar = player.Avatar ?? "",
                    AvatarMedium = player.AvatarMedium ?? "",
                    AvatarFull = player.AvatarFull ?? ""
                };

                return Ok(new ApiResponse<SteamUserInfo>
                {
                    Success = true,
                    Data = userInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Steam user info for Steam ID: {SteamId}", steamId);
                return StatusCode(500, new ApiResponse<SteamUserInfo>
                {
                    Success = false,
                    Code = ErrorCode.ServerError
                });
            }
        }

        /// <summary>
        /// Handles Steam authentication errors
        /// </summary>
        [HttpGet("error")]
        public IActionResult Error()
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Code = ErrorCode.Unauthorized
            });
        }
    }
}
