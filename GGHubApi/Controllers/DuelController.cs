using GGHubApi.Services;
using GGHubShared.Enums;
using GGHubShared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GGHubApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DuelsController : ControllerBase
    {
        private readonly IDuelService _duelService;
        private readonly ILogger<DuelsController> _logger;

        public DuelsController(IDuelService duelService, ILogger<DuelsController> logger)
        {
            _duelService = duelService;
            _logger = logger;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<DuelDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _duelService.GetByIdAsync(id, cancellationToken);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("{id:guid}/full")]
        public async Task<ActionResult<ApiResponse<DuelDto>>> GetFullDuel(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _duelService.GetFullDuelAsync(id, cancellationToken);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<DuelDto>>>> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DuelFormat? format = null,
            [FromQuery] DuelStatus? status = null,
            [FromQuery] Guid? userId = null,
            CancellationToken cancellationToken = default)
        {
            ApiResponse<PagedResult<DuelDto>> result;

            if (format.HasValue || status.HasValue || userId.HasValue)
            {
                result = await _duelService.GetPagedWithFiltersAsync(pageNumber, pageSize, format, status, userId, cancellationToken);
            }
            else
            {
                result = await _duelService.GetPagedAsync(pageNumber, pageSize, cancellationToken);
            }

            return Ok(result);
        }

        [HttpGet("available")]
        public async Task<ActionResult<ApiResponse<List<DuelDto>>>> GetAvailable(CancellationToken cancellationToken = default)
        {
            var result = await _duelService.GetAvailableDuelsAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<ApiResponse<List<DuelDto>>>> GetUserDuels(Guid userId, CancellationToken cancellationToken = default)
        {
            var result = await _duelService.GetUserDuelsAsync(userId, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<DuelDto>>> Create(
            [FromBody] CreateDuelRequest request,
            [FromQuery] Guid createdBy,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _duelService.CreateDuelAsync(request, createdBy, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }

        [HttpPost("{id:guid}/join")]
        public async Task<ActionResult<ApiResponse<DuelDto>>> JoinDuel(
            Guid id,
            [FromBody] JoinDuelRequest request,
            [FromQuery] Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            request.DuelId = id;
            var result = await _duelService.JoinDuelAsync(request, userId, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("join-by-link")]
        public async Task<ActionResult<ApiResponse<DuelDto>>> JoinDuelByInviteLink(
            [FromBody] JoinDuelByInviteLinkRequest request,
            [FromQuery] Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _duelService.JoinByInviteLinkAsync(request.InviteLink, userId, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/pay")]
        public async Task<ActionResult<ApiResponse<DuelDto>>> PayEntryFee(
            Guid id,
            [FromQuery] Guid userId,
            [FromQuery] PaymentProvider provider,
            CancellationToken cancellationToken = default)
        {
            var result = await _duelService.PayEntryFeeAsync(id, userId, provider, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/leave")]
        public async Task<ActionResult<ApiResponse<bool>>> LeaveDuel(
            Guid id,
            [FromQuery] Guid userId,
            CancellationToken cancellationToken = default)
        {
            var result = await _duelService.LeaveDuelAsync(id, userId, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/start")]
        public async Task<ActionResult<ApiResponse<bool>>> StartDuel(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _duelService.StartDuelAsync(id, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/ready")]
        public async Task<ActionResult<ApiResponse<bool>>> ConfirmReady(
            Guid id,
            [FromQuery] Guid userId,
            CancellationToken cancellationToken = default)
        {
            var result = await _duelService.ConfirmReadyAsync(id, userId, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/complete")]
        public async Task<ActionResult<ApiResponse<bool>>> CompleteDuel(
            Guid id,
            [FromBody] CompleteDuelRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _duelService.CompleteDuelAsync(id, request.WinnerId, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelDuel(
            Guid id,
            [FromQuery] Guid userId,
            CancellationToken cancellationToken = default)
        {
            var result = await _duelService.CancelDuelAsync(id, userId, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{id:guid}/status")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(
            Guid id,
            [FromBody] UpdateDuelStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _duelService.UpdateStatusAsync(id, request.Status, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/invite-link")]
        public async Task<ActionResult<ApiResponse<string>>> GenerateInviteLink(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _duelService.GenerateInviteLinkAsync(id, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/forfeit")]
        public async Task<ActionResult<ApiResponse<DuelForfeitResult>>> ForfeitDuel(
            Guid id,
            [FromBody] ForfeitDuelRequest request,
            [FromQuery] Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _duelService.ForfeitDuelAsync(id, userId, request.Reason, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}