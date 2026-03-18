using GGHubDb.Services;
using GGHubShared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GGHubApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly IMatchMetricsService _metricsService;

    public MetricsController(IMatchMetricsService metricsService)
    {
        _metricsService = metricsService;
    }

    [HttpGet("global")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<GlobalMatchMetricsDto>>> GetGlobal(CancellationToken cancellationToken = default)
    {
        var result = await _metricsService.GetGlobalMetricsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("user/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserMatchMetricsDto>>> GetForUser(Guid userId, CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && (userIdClaim == null || userIdClaim != userId.ToString()))
            return Forbid();

        var result = await _metricsService.GetUserMetricsAsync(userId, cancellationToken);
        return Ok(result);
    }
}
