using Athlo.AuthService.Services;
using Athlo.Models.DTOs.Admin;
using Athlo.Shared.Authorization;
using Athlo.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.AuthService.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = AthloPolicies.SuperAdminOnly)]
[EnableRateLimiting("api")]
public class AdminController(
    IAdminService adminService,
    IValidator<CreateAdminRequest> createAdminValidator) : ControllerBase
{
    [HttpGet("admins")]
    public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> GetAdmins(CancellationToken ct)
    {
        return Ok(await adminService.GetAdminsAsync(ct));
    }

    [HttpPost("admins")]
    public async Task<ActionResult<AdminUserDto>> CreateAdmin([FromBody] CreateAdminRequest request, CancellationToken ct)
    {
        var error = await createAdminValidator.ToValidationErrorAsync(request, ct);
        if (error is not null)
            return (ActionResult)error;

        var admin = await adminService.CreateAdminAsync(request, ct);
        return CreatedAtAction(nameof(GetAdmins), new { }, admin);
    }

    [HttpDelete("admins/{id:guid}")]
    public async Task<IActionResult> RemoveAdmin(Guid id, CancellationToken ct)
    {
        await adminService.RemoveAdminAsync(id, ct);
        return NoContent();
    }
}
