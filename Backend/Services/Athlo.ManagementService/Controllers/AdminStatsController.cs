using Athlo.ManagementService.Services;
using Athlo.Models.DTOs.Admin;
using Athlo.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.ManagementService.Controllers;

/// <summary>
/// Provides dashboard statistics for super-admin users.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Policy = AthloPolicies.SuperAdminOnly)]
[EnableRateLimiting("api")]
public class AdminStatsController(IAdminStatsService adminStatsService) : ControllerBase
{
    /// <summary>
    /// Returns aggregated platform statistics for the admin dashboard.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Dashboard stats including user counts and activity metrics.</returns>
    /// <response code="200">Stats retrieved successfully.</response>
    /// <response code="403">Caller is not a super-admin.</response>
    /// <remarks>Requires the SuperAdminOnly authorization policy.</remarks>
    [HttpGet("stats")]
    public async Task<ActionResult<AdminDashboardStatsDto>> GetStats(CancellationToken ct) =>
        Ok(await adminStatsService.GetDashboardStatsAsync(ct));
}
