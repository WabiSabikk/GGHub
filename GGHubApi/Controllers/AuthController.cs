using GGHubApi.Services;
using GGHubDb.Repos;
using GGHubDb.Services;
using GGHubShared.Models;
using GGHubShared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GGHubApi.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserService userService,
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher,
        ILogger<AuthController> logger)
    {
        _userService = userService;
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userResult = await _userService.GetByEmailAsync(request.Email, cancellationToken);
        if (!userResult.Success || userResult.Data == null)
        {
            _logger.LogWarning("Login failed for {Email}: user not found", request.Email);
            return Unauthorized(new ApiResponse<AuthResponse>
            {
                Success = false,
                Code = ErrorCode.InvalidCredentials
            });
        }

        var userEntity = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (userEntity == null || userEntity.PasswordHash == null || !_passwordHasher.VerifyPassword(request.PasswordHash, userEntity.PasswordHash))
        {
            _logger.LogWarning("Login failed for {Email}: invalid password", request.Email);
            return Unauthorized(new ApiResponse<AuthResponse>
            {
                Success = false,
                Code = ErrorCode.InvalidCredentials
            });
        }

        var token = _jwtTokenService.GenerateToken(userEntity.Id, userEntity.Role.ToString());

        var response = new AuthResponse
        {
            Token = token,
            User = userResult.Data
        };

        return Ok(new ApiResponse<AuthResponse>
        {
            Success = true,
            Data = response
        });
    }
}
