using GGHubDb.Services;
using GGHubShared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GGHubApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("{id:guid}")]

        public async Task<ActionResult<ApiResponse<UserDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _userService.GetByIdAsync(id, cancellationToken);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("by-email/{email}")]

        public async Task<ActionResult<ApiResponse<UserDto>>> GetByEmail(string email, CancellationToken cancellationToken = default)
        {
            var result = await _userService.GetByEmailAsync(email, cancellationToken);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("by-username/{username}")]

        public async Task<ActionResult<ApiResponse<UserDto>>> GetByUsername(string username, CancellationToken cancellationToken = default)
        {
            var result = await _userService.GetByUsernameAsync(username, cancellationToken);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("by-telegram/{telegramChatId:long}")]

        public async Task<ActionResult<ApiResponse<UserDto>>> GetByTelegramChatId(long telegramChatId, CancellationToken cancellationToken = default)
        {
            var result = await _userService.GetByTelegramChatIdAsync(telegramChatId, cancellationToken);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost]

        public async Task<ActionResult<ApiResponse<UserDto>>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.CreateUserAsync(request.Username, request.Email, request.PasswordHash, cancellationToken: cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }

        [HttpPut("{id:guid}")]

        public async Task<ActionResult<ApiResponse<UserDto>>> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.UpdateUserAsync(id, request.Username, request.Email, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/balance")]

        public async Task<ActionResult<ApiResponse<bool>>> UpdateBalance(Guid id, [FromBody] UpdateBalanceRequest request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.UpdateBalanceAsync(id, request.Amount, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/stats")]

        public async Task<ActionResult<ApiResponse<bool>>> UpdateStats(Guid id, [FromBody] UpdateStatsRequest request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.UpdateStatsAsync(id, request.IsWin, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/link-steam")]

        public async Task<ActionResult<ApiResponse<bool>>> LinkSteamAccount(Guid id, [FromBody] LinkSteamRequest request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.LinkSteamAccountAsync(id, request.SteamId, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/link-telegram")]

        public async Task<ActionResult<ApiResponse<bool>>> LinkTelegramAccount(Guid id, [FromBody] LinkTelegramRequest request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.LinkTelegramAccountAsync(id, request.TelegramUsername, request.TelegramChatId, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/activate-prime")]

        public async Task<ActionResult<ApiResponse<bool>>> ActivatePrime(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _userService.ActivatePrimeAsync(id, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/deactivate-prime")]

        public async Task<ActionResult<ApiResponse<bool>>> DeactivatePrime(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _userService.DeactivatePrimeAsync(id, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("top-players")]
        public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetTopPlayers([FromQuery] int count = 10, CancellationToken cancellationToken = default)
        {
            var result = await _userService.GetTopPlayersByRatingAsync(count, cancellationToken);
            return Ok(result);
        }

        [HttpGet("check-email/{email}")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckEmailAvailability(string email, [FromQuery] Guid? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            var result = await _userService.IsEmailAvailableAsync(email, excludeUserId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("check-username/{username}")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckUsernameAvailability(string username, [FromQuery] Guid? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            var result = await _userService.IsUsernameAvailableAsync(username, excludeUserId, cancellationToken);
            return Ok(result);
        }
    }
}
