using Athlo.AuthService.Services;
using Athlo.Models.DTOs.Auth;
using Athlo.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.AuthService.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController(
    IAuthService authService,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator,
    IValidator<RefreshTokenRequest> refreshTokenValidator,
    IValidator<ChangePasswordRequest> changePasswordValidator,
    IValidator<ForgotPasswordRequest> forgotPasswordValidator,
    IValidator<ResetPasswordRequest> resetPasswordValidator,
    IValidator<UpdateProfileRequest> updateProfileValidator,
    IValidator<UserPreferencesDto> preferencesValidator) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var error = await registerValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await authService.RegisterAsync(request, ct));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var error = await loginValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await authService.LoginAsync(request, ct));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var error = await refreshTokenValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await authService.RefreshAsync(request.RefreshToken, ct));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var error = await refreshTokenValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        await authService.LogoutAsync(User.GetUserId(), request.RefreshToken, User, ct);
        return NoContent();
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserProfileResponse>> GetProfile(CancellationToken ct)
    {
        return Ok(await authService.GetProfileAsync(User.GetUserId(), ct));
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var error = await updateProfileValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return error;

        return Ok(await authService.UpdateProfileAsync(User.GetUserId(), request, ct));
    }

    [HttpGet("preferences")]
    [Authorize]
    public async Task<ActionResult<UserPreferencesDto>> GetPreferences(CancellationToken ct) =>
        Ok(await authService.GetPreferencesAsync(User.GetUserId(), ct));

    [HttpPut("preferences")]
    [Authorize]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UserPreferencesDto request,
        CancellationToken ct)
    {
        var error = await preferencesValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return error;

        return Ok(await authService.UpdatePreferencesAsync(User.GetUserId(), request, ct));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var error = await changePasswordValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return error;

        await authService.ChangePasswordAsync(User.GetUserId(), request, User, ct);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        var error = await forgotPasswordValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await authService.ForgotPasswordAsync(request, ct));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var error = await resetPasswordValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return error;

        await authService.ResetPasswordAsync(request, ct);
        return NoContent();
    }
}
