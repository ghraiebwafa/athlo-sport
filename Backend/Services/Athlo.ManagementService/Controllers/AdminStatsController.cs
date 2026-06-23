using Athlo.ManagementService.Services;
using Athlo.Models.DTOs.Admin;
using Athlo.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.ManagementService.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = AthloPolicies.SuperAdminOnly)]
[EnableRateLimiting("api")]
public class AdminStatsController(IAdminStatsService adminStatsService) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<ActionResult<AdminDashboardStatsDto>> GetStats(CancellationToken ct) =>
        Ok(await adminStatsService.GetDashboardStatsAsync(ct));
}
