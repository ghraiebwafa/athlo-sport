using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Athlo.Models.DTOs.Auth;

namespace Athlo.AuthService.Services;

/// <summary>
/// Authentication and user-account operations: registration, login, token lifecycle,
/// profile management, and password recovery.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user account and returns access and refresh tokens.
    /// </summary>
    /// <param name="request">Registration details including email, password, and fitness profile.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Authentication response with tokens and user profile.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.ConflictException">
    /// Thrown when the email is reserved or already registered.
    /// </exception>
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    /// <summary>
    /// Authenticates a user with email and password and returns new tokens.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Authentication response with tokens and user profile.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.UnauthorizedException">
    /// Thrown for unknown email or incorrect password (same message to avoid account enumeration).
    /// </exception>
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

    /// <summary>
    /// Exchanges a valid refresh token for a new access/refresh token pair (rotation).
    /// </summary>
    /// <param name="refreshToken">The opaque refresh token value.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Authentication response with rotated tokens.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.UnauthorizedException">
    /// Thrown when the token is invalid, expired, or already revoked. Reuse of a revoked
    /// refresh token revokes all refresh tokens for that user as a security measure.
    /// </exception>
    Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>
    /// Signs the user out by revoking the supplied refresh token and blacklisting the
    /// current access token until its natural expiry.
    /// </summary>
    /// <param name="userId">Authenticated user identifier.</param>
    /// <param name="refreshToken">Refresh token to invalidate.</param>
    /// <param name="principal">Current request principal; used to extract the access-token JTI and expiry.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>
    /// The refresh token is revoked only when it belongs to <paramref name="userId"/>.
    /// The access token identified by its JTI claim is added to the revocation denylist
    /// via <see cref="Athlo.Shared.Security.IAccessTokenRevocationService"/>.
    /// </remarks>
    Task LogoutAsync(Guid userId, string refreshToken, ClaimsPrincipal principal, CancellationToken ct = default);

    /// <summary>
    /// Returns the profile for the authenticated user.
    /// </summary>
    /// <param name="userId">Authenticated user identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>User profile data.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.NotFoundException">Thrown when the user does not exist.</exception>
    Task<UserProfileResponse> GetProfileAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Updates mutable profile fields for the authenticated user.
    /// </summary>
    /// <param name="userId">Authenticated user identifier.</param>
    /// <param name="request">Fields to update; omitted fields are left unchanged.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Updated user profile.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.NotFoundException">Thrown when the user does not exist.</exception>
    Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);

    /// <summary>
    /// Returns application preferences for the authenticated user.
    /// </summary>
    /// <param name="userId">Authenticated user identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>User preferences.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.NotFoundException">Thrown when the user does not exist.</exception>
    Task<UserPreferencesDto> GetPreferencesAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Persists application preferences for the authenticated user.
    /// </summary>
    /// <param name="userId">Authenticated user identifier.</param>
    /// <param name="request">Preferences to save.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Normalized and saved preferences.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.NotFoundException">Thrown when the user does not exist.</exception>
    Task<UserPreferencesDto> UpdatePreferencesAsync(Guid userId, UserPreferencesDto request, CancellationToken ct = default);

    /// <summary>
    /// Changes the authenticated user's password and invalidates all existing sessions.
    /// </summary>
    /// <param name="userId">Authenticated user identifier.</param>
    /// <param name="request">Current and new password values.</param>
    /// <param name="principal">Current request principal; used to revoke the active access token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Athlo.Shared.Exceptions.NotFoundException">Thrown when the user does not exist.</exception>
    /// <exception cref="Athlo.Shared.Exceptions.UnauthorizedException">Thrown when the current password is incorrect.</exception>
    /// <exception cref="Athlo.Shared.Exceptions.AppException">
    /// Thrown with HTTP 403 when the user is the super admin (password must be changed via configuration).
    /// </exception>
    /// <remarks>
    /// On success, all refresh tokens are revoked and all access tokens issued before this
    /// moment are invalidated via <see cref="Athlo.Shared.Security.IAccessTokenRevocationService.RevokeAllForUser"/>.
    /// The current access token is also explicitly revoked by JTI.
    /// </remarks>
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, ClaimsPrincipal principal, CancellationToken ct = default);

    /// <summary>
    /// Initiates a password-reset flow by email. Does not reveal whether the email is registered.
    /// </summary>
    /// <param name="request">Email address to send the reset link to.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Generic response; may include a reset token in development when configured.</returns>
    /// <remarks>
    /// Returns the same empty response for unknown emails and super-admin accounts to avoid
    /// account enumeration. Existing reset tokens for the user are invalidated before a new one is issued.
    /// </remarks>
    Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);

    /// <summary>
    /// Completes a password reset using a one-time token and invalidates all existing sessions.
    /// </summary>
    /// <param name="request">Reset token and new password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Athlo.Shared.Exceptions.UnauthorizedException">Thrown when the reset token is invalid or expired.</exception>
    /// <exception cref="Athlo.Shared.Exceptions.AppException">
    /// Thrown with HTTP 403 when the account is the super admin.
    /// </exception>
    /// <remarks>
    /// On success, all refresh tokens are revoked and all access tokens issued before this
    /// moment are invalidated via <see cref="Athlo.Shared.Security.IAccessTokenRevocationService.RevokeAllForUser"/>.
    /// </remarks>
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);

    /// <summary>
    /// Builds a JSON-serializable export of the user's profile, preferences, workouts, and saved programs.
    /// </summary>
    Task<UserDataExportDto> ExportDataAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Permanently deletes the user account and cascaded workout data after password confirmation.
    /// </summary>
    /// <remarks>
    /// Super admin accounts cannot be deleted. All sessions are revoked before deletion.
    /// </remarks>
    Task DeleteAccountAsync(Guid userId, DeleteAccountRequest request, ClaimsPrincipal principal, CancellationToken ct = default);
}
