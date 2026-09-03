using Athlo.ManagementService.Services;
using Athlo.Models.DTOs.Notifications;
using Athlo.Repositories;
using Athlo.Repositories.Notifications;
using Athlo.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.ManagementService.Controllers;

/// <summary>Registers device push tokens for Expo / native notifications.</summary>
[ApiController]
[Route("api/devices")]
[Authorize]
[EnableRateLimiting("api")]
public class DevicesController(
    IDevicePushTokenRepository tokenRepository,
    IUnitOfWork unitOfWork,
    IValidator<RegisterDeviceTokenRequest> validator) : ControllerBase
{
    /// <summary>Registers or refreshes a push token for the current user.</summary>
    [HttpPost("push-token")]
    public async Task<IActionResult> Register([FromBody] RegisterDeviceTokenRequest request, CancellationToken ct)
    {
        var error = await validator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return error;

        await tokenRepository.UpsertAsync(
            User.GetUserId(),
            request.Token.Trim(),
            string.IsNullOrWhiteSpace(request.Platform) ? "unknown" : request.Platform.Trim().ToLowerInvariant(),
            ct);
        await unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Removes a push token (e.g. on logout).</summary>
    [HttpDelete("push-token")]
    public async Task<IActionResult> Unregister([FromBody] RegisterDeviceTokenRequest request, CancellationToken ct)
    {
        var error = await validator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return error;

        await tokenRepository.RemoveAsync(User.GetUserId(), request.Token.Trim(), ct);
        await unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }
}
