using Athlo.AuthService.Services;
using Athlo.Models.DTOs.Auth;
using Athlo.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.AuthService.Controllers;

/// <summary>
/// Handles user authentication, registration, profile management, and password operations.
/// </summary>
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
    IValidator<UserPreferencesDto> preferencesValidator,
    IValidator<DeleteAccountRequest> deleteAccountValidator) : ControllerBase
{
    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">Registration details including email and password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Authentication tokens and user info on success.</returns>
    /// <response code="200">User registered successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <remarks>Anonymous endpoint. Rate-limited under the "auth" policy.</remarks>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var error = await registerValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await authService.RegisterAsync(request, ct));
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Authentication tokens and user info on success.</returns>
    /// <response code="200">Login successful.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Invalid email or password.</response>
    /// <remarks>Anonymous endpoint. Rate-limited under the "auth" policy.</remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var error = await loginValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await authService.LoginAsync(request, ct));
    }

    /// <summary>
    /// Issues new access and refresh tokens using a valid refresh token.
    /// </summary>
    /// <param name="request">The refresh token to exchange.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>New authentication tokens.</returns>
    /// <response code="200">Tokens refreshed successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Invalid or expired refresh token.</response>
    /// <remarks>Anonymous endpoint.</remarks>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var error = await refreshTokenValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await authService.RefreshAsync(request.RefreshToken, ct));
    }

    /// <summary>
    /// Revokes the provided refresh token and ends the session.
    /// </summary>
    /// <param name="request">The refresh token to revoke.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Logout successful.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Authentication required.</response>
    /// <remarks>Requires a valid JWT bearer token.</remarks>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var error = await refreshTokenValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        await authService.LogoutAsync(User.GetUserId(), request.RefreshToken, User, ct);
        return NoContent();
    }

    /// <summary>
    /// Retrieves the authenticated user's profile.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The user's profile information.</returns>
    /// <response code="200">Profile retrieved successfully.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">User not found.</response>
    /// <remarks>Requires a valid JWT bearer token.</remarks>
    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserProfileResponse>> GetProfile(CancellationToken ct)
    {
        return Ok(await authService.GetProfileAsync(User.GetUserId(), ct));
    }

    /// <summary>
    /// Updates the authenticated user's profile fields.
    /// </summary>
    /// <param name="request">Updated profile values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated profile.</returns>
    /// <response code="200">Profile updated successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">User not found.</response>
    /// <remarks>Requires a valid JWT bearer token.</remarks>
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var error = await updateProfileValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return error;

        return Ok(await authService.UpdateProfileAsync(User.GetUserId(), request, ct));
    }

    /// <summary>
    /// Retrieves the authenticated user's preferences.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The user's preferences.</returns>
    /// <response code="200">Preferences retrieved successfully.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">User not found.</response>
    /// <remarks>Requires a valid JWT bearer token.</remarks>
    [HttpGet("preferences")]
    [Authorize]
    public async Task<ActionResult<UserPreferencesDto>> GetPreferences(CancellationToken ct) =>
        Ok(await authService.GetPreferencesAsync(User.GetUserId(), ct));

    /// <summary>
    /// Updates the authenticated user's preferences.
    /// </summary>
    /// <param name="request">Updated preference values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated preferences.</returns>
    /// <response code="200">Preferences updated successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">User not found.</response>
    /// <remarks>Requires a valid JWT bearer token.</remarks>
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

    /// <summary>
    /// Changes the authenticated user's password.
    /// </summary>
    /// <param name="request">Current and new password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Password changed successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Authentication required or current password is incorrect.</response>
    /// <response code="404">User not found.</response>
    /// <remarks>Requires a valid JWT bearer token.</remarks>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var error = await changePasswordValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return error;

        await authService.ChangePasswordAsync(User.GetUserId(), request, User, ct);
        return NoContent();
    }

    /// <summary>
    /// Initiates a password reset by sending a reset link to the provided email.
    /// </summary>
    /// <param name="request">The email address associated with the account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A confirmation message (always returned regardless of whether the email exists).</returns>
    /// <response code="200">Request accepted.</response>
    /// <response code="400">Validation failed.</response>
    /// <remarks>Anonymous endpoint. Does not reveal whether the email is registered.</remarks>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        var error = await forgotPasswordValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await authService.ForgotPasswordAsync(request, ct));
    }

    /// <summary>
    /// Resets a user's password using a valid reset token.
    /// </summary>
    /// <param name="request">Reset token and new password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Password reset successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Invalid or expired reset token.</response>
    /// <remarks>Anonymous endpoint.</remarks>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var error = await resetPasswordValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return error;

        await authService.ResetPasswordAsync(request, ct);
        return NoContent();
    }

    /// <summary>
    /// Exports the authenticated user's Athlo data (profile, preferences, workouts, saved programs).
    /// </summary>
    /// <response code="200">Export payload ready for download.</response>
    /// <response code="401">Authentication required.</response>
    [HttpGet("account/export")]
    [Authorize]
    public async Task<ActionResult<UserDataExportDto>> ExportAccount(CancellationToken ct) =>
        Ok(await authService.ExportDataAsync(User.GetUserId(), ct));

    /// <summary>
    /// Permanently deletes the authenticated user's account after password confirmation.
    /// </summary>
    /// <remarks>
    /// Cascades workout sessions, set logs, saved programs, and tokens. Super admin cannot be deleted.
    /// </remarks>
    /// <response code="204">Account deleted.</response>
    /// <response code="401">Incorrect password or unauthenticated.</response>
    /// <response code="403">Super admin account.</response>
    [HttpDelete("account")]
    [Authorize]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request, CancellationToken ct)
    {
        var error = await deleteAccountValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return error;

        await authService.DeleteAccountAsync(User.GetUserId(), request, User, ct);
        return NoContent();
    }
}
