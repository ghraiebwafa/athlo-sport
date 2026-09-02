using Athlo.AuthService.Services;
using Athlo.Models.DTOs.Admin;
using Athlo.Models.DTOs.Workouts;
using Athlo.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.AuthService.Controllers;

/// <summary>
/// Provides read-only access to registered user accounts for admin users.
/// </summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = AthloPolicies.AdminOrSuperAdmin)]
[EnableRateLimiting("api")]
public class AdminUsersController(IAdminService adminService) : ControllerBase
{
    /// <summary>
    /// Returns a paginated list of all registered users.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A page of user list items.</returns>
    /// <response code="200">Users retrieved successfully.</response>
    /// <response code="403">Caller is not an admin or super-admin.</response>
    /// <remarks>Requires the AdminOrSuperAdmin authorization policy.</remarks>
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserListItemDto>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        return Ok(await adminService.GetUsersAsync(page, pageSize, ct));
    }

    /// <summary>
    /// Retrieves detailed information for a single user.
    /// </summary>
    /// <param name="id">The user's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The user's full details.</returns>
    /// <response code="200">User retrieved successfully.</response>
    /// <response code="403">Caller is not an admin or super-admin.</response>
    /// <response code="404">User not found.</response>
    /// <remarks>Requires the AdminOrSuperAdmin authorization policy.</remarks>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDetailDto>> GetUserById(Guid id, CancellationToken ct)
    {
        return Ok(await adminService.GetUserByIdAsync(id, ct));
    }
}
