using Athlo.ManagementService.Services;
using Athlo.Models.DTOs.Progress;
using Athlo.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.ManagementService.Controllers;

[ApiController]
[Route("api/progress")]
[Authorize]
[EnableRateLimiting("api")]
public class ProgressController(IProgressService progressService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProgressResponse>> GetProgress(CancellationToken ct)
    {
        return Ok(await progressService.GetProgressAsync(User.GetUserId(), ct));
    }
}
