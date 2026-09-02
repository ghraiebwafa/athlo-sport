using Athlo.ManagementService.Services;
using Athlo.Models.DTOs.Progress;
using Athlo.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.ManagementService.Controllers;

/// <summary>
/// Provides the authenticated user's fitness progress and statistics.
/// </summary>
[ApiController]
[Route("api/progress")]
[Authorize]
[EnableRateLimiting("api")]
public class ProgressController(IProgressService progressService) : ControllerBase
{
    /// <summary>
    /// Retrieves the authenticated user's overall workout progress.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Aggregated progress data including completed workouts and streaks.</returns>
    /// <response code="200">Progress retrieved successfully.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">User not found.</response>
    [HttpGet]
    public async Task<ActionResult<ProgressResponse>> GetProgress(CancellationToken ct)
    {
        return Ok(await progressService.GetProgressAsync(User.GetUserId(), ct));
    }
}
