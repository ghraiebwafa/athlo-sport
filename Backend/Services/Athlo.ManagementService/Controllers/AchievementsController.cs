using Athlo.ManagementService.Services;
using Athlo.Models.DTOs.Achievements;
using Athlo.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.ManagementService.Controllers;

/// <summary>User achievement catalog and unlock status.</summary>
[ApiController]
[Route("api/achievements")]
[Authorize]
[EnableRateLimiting("api")]
public class AchievementsController(IAchievementService achievementService) : ControllerBase
{
    /// <summary>Lists all achievements with unlock state for the current user.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AchievementDto>>> GetMine(CancellationToken ct) =>
        Ok(await achievementService.GetForUserAsync(User.GetUserId(), ct));
}
