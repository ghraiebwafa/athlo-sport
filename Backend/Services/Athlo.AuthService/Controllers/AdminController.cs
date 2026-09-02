using Athlo.AuthService.Services;
using Athlo.Models.DTOs.Admin;
using Athlo.Shared.Authorization;
using Athlo.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.AuthService.Controllers;

/// <summary>
/// Manages admin accounts. Restricted to super-admin users only.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Policy = AthloPolicies.SuperAdminOnly)]
[EnableRateLimiting("api")]
public class AdminController(
    IAdminService adminService,
    IValidator<CreateAdminRequest> createAdminValidator) : ControllerBase
{
    /// <summary>
    /// Lists all admin users.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of admin user records.</returns>
    /// <response code="200">Admins retrieved successfully.</response>
    /// <response code="403">Caller is not a super-admin.</response>
    /// <remarks>Requires the SuperAdminOnly authorization policy.</remarks>
    [HttpGet("admins")]
    public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> GetAdmins(CancellationToken ct)
    {
        return Ok(await adminService.GetAdminsAsync(ct));
    }

    /// <summary>
    /// Creates a new admin user.
    /// </summary>
    /// <param name="request">Admin account details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created admin user.</returns>
    /// <response code="201">Admin created successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="403">Caller is not a super-admin.</response>
    /// <remarks>Requires the SuperAdminOnly authorization policy.</remarks>
    [HttpPost("admins")]
    public async Task<ActionResult<AdminUserDto>> CreateAdmin([FromBody] CreateAdminRequest request, CancellationToken ct)
    {
        var error = await createAdminValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null)
            return (ActionResult)error;

        var admin = await adminService.CreateAdminAsync(request, ct);
        return CreatedAtAction(nameof(GetAdmins), new { }, admin);
    }

    /// <summary>
    /// Removes an admin user by ID.
    /// </summary>
    /// <param name="id">The admin user's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Admin removed successfully.</response>
    /// <response code="403">Caller is not a super-admin.</response>
    /// <response code="404">Admin not found.</response>
    /// <remarks>Requires the SuperAdminOnly authorization policy.</remarks>
    [HttpDelete("admins/{id:guid}")]
    public async Task<IActionResult> RemoveAdmin(Guid id, CancellationToken ct)
    {
        await adminService.RemoveAdminAsync(id, ct);
        return NoContent();
    }
}
