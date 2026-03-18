using GGHubApi.Services;
using GGHubDb.Models;
using GGHubShared.Models;
using Microsoft.AspNetCore.Mvc;

namespace GGHubApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly IMatchConfigService _matchConfigService;
        private readonly IServerService _regionService;

        public ConfigController(IMatchConfigService matchConfigService, IServerService regionService)
        {
            _matchConfigService = matchConfigService;
            _regionService = regionService;
        }

        [HttpGet("formats")]
        public async Task<ActionResult<ApiResponse<List<DuelFormatConfig>>>> GetAvailableFormats(CancellationToken cancellationToken = default)
        {
            var result = await _matchConfigService.GetAvailableFormatsAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("regions")]
        public async Task<ActionResult<ApiResponse<List<ServerRegionConfig>>>> GetAvailableRegions(CancellationToken cancellationToken = default)
        {
            var result = await _regionService.GetAvailableRegionsAsync(cancellationToken);
            return Ok(result);
        }

        [HttpPost("validate")]
        public async Task<ActionResult<ApiResponse<bool>>> ValidateCustomSettings([FromBody] CreateDuelRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _matchConfigService.ValidateCustomSettingsAsync(request.Format, request, cancellationToken);
            return Ok(result);
        }
    }
}
