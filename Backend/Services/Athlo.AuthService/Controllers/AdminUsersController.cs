using Athlo.AuthService.Services;
using Athlo.Models.DTOs.Admin;
using Athlo.Models.DTOs.Workouts;
using Athlo.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.AuthService.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = AthloPolicies.AdminOrSuperAdmin)]
[EnableRateLimiting("api")]
public class AdminUsersController(IAdminService adminService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserListItemDto>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        return Ok(await adminService.GetUsersAsync(page, pageSize, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDetailDto>> GetUserById(Guid id, CancellationToken ct)
    {
        return Ok(await adminService.GetUserByIdAsync(id, ct));
    }
}
