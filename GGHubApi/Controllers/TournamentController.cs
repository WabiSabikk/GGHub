using GGHubApi.Services;
using GGHubShared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GGHubApi.Controllers
{
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TournamentController : ControllerBase
    {
        private readonly ITournamentService _tournamentService;
        private readonly ILogger<TournamentController> _logger;

        public TournamentController(ITournamentService tournamentService, ILogger<TournamentController> logger)
        {
            _tournamentService = tournamentService;
            _logger = logger;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<TournamentDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _tournamentService.GetByIdAsync(id, cancellationToken);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("{id:guid}/full")]
        public async Task<ActionResult<ApiResponse<TournamentDto>>> GetFullTournament(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _tournamentService.GetFullTournamentAsync(id, cancellationToken);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<TournamentDto>>>> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _tournamentService.GetPagedAsync(pageNumber, pageSize, cancellationToken);
            return Ok(result);
        }

        [HttpGet("available")]
        public async Task<ActionResult<ApiResponse<List<TournamentDto>>>> GetAvailable(CancellationToken cancellationToken = default)
        {
            var result = await _tournamentService.GetAvailableTournamentsAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<ApiResponse<List<TournamentDto>>>> GetUserTournaments(Guid userId, CancellationToken cancellationToken = default)
        {
            var result = await _tournamentService.GetUserTournamentsAsync(userId, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<TournamentDto>>> Create(
            [FromBody] CreateTournamentRequest request,
            [FromQuery] Guid createdBy,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _tournamentService.CreateTournamentAsync(request, createdBy, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<TournamentDto>>> Update(
            Guid id,
            [FromBody] UpdateTournamentRequest request,
            [FromQuery] Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _tournamentService.UpdateTournamentAsync(id, request, userId, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/join")]
        public async Task<ActionResult<ApiResponse<TournamentTeamDto>>> JoinTournament(
            Guid id,
            [FromBody] JoinTournamentRequest request,
            [FromQuery] Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            request.TournamentId = id;
            var result = await _tournamentService.JoinTournamentAsync(request, userId, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("join-by-token")]
        public async Task<ActionResult<ApiResponse<TournamentTeamDto>>> JoinTournamentByToken(
            [FromBody] JoinTournamentByTokenRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _tournamentService.JoinTournamentByTokenAsync(request, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/leave")]
        public async Task<ActionResult<ApiResponse<bool>>> LeaveTournament(
            Guid id,
            [FromQuery] Guid userId,
            CancellationToken cancellationToken = default)
        {
            var result = await _tournamentService.LeaveTournamentAsync(id, userId, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/start")]
        public async Task<ActionResult<ApiResponse<bool>>> StartTournament(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _tournamentService.StartTournamentAsync(id, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelTournament(
            Guid id,
            [FromQuery] Guid userId,
            CancellationToken cancellationToken = default)
        {
            var result = await _tournamentService.CancelTournamentAsync(id, userId, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{id:guid}/bracket")]
        public async Task<ActionResult<ApiResponse<TournamentBracketDto>>> GetBracket(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _tournamentService.GetBracketAsync(id, cancellationToken);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/invite-link")]
        public async Task<ActionResult<ApiResponse<string>>> GenerateInviteLink(
            Guid id,
            [FromQuery] Guid userId,
            CancellationToken cancellationToken = default)
        {
            var result = await _tournamentService.GenerateInviteLinkAsync(id, userId, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("payment")]
        public async Task<ActionResult<ApiResponse<TournamentPaymentDto>>> CreatePayment(
            [FromBody] TournamentPaymentRequest request,
            [FromQuery] Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _tournamentService.CreatePaymentAsync(request, userId, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}