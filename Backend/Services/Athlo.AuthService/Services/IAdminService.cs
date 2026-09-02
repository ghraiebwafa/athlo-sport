using Athlo.Models.DTOs.Admin;
using Athlo.Models.DTOs.Workouts;

namespace Athlo.AuthService.Services;

/// <summary>
/// Administrative operations for managing admin accounts and viewing registered users.
/// Requires an authenticated admin or super-admin caller.
/// </summary>
public interface IAdminService
{
    /// <summary>
    /// Returns all users with the Admin or SuperAdmin role.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of admin user summaries.</returns>
    Task<IReadOnlyList<AdminUserDto>> GetAdminsAsync(CancellationToken ct = default);

    /// <summary>
    /// Promotes a new admin account by creating a user with the Admin role.
    /// </summary>
    /// <param name="request">Admin account details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created admin user.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.ConflictException">
    /// Thrown when the email is reserved for the super admin or already exists.
    /// </exception>
    Task<AdminUserDto> CreateAdminAsync(CreateAdminRequest request, CancellationToken ct = default);

    /// <summary>
    /// Demotes an admin to a regular user and revokes all of their active sessions.
    /// </summary>
    /// <param name="adminId">Identifier of the admin to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Athlo.Shared.Exceptions.NotFoundException">
    /// Thrown when no admin with the given identifier exists (including when the user
    /// exists but does not have the Admin role).
    /// </exception>
    /// <exception cref="Athlo.Shared.Exceptions.AppException">
    /// Thrown with HTTP 403 when attempting to remove the super admin account.
    /// </exception>
    /// <remarks>
    /// On success, all refresh tokens are revoked and all access tokens issued before this
    /// moment are invalidated via <see cref="Athlo.Shared.Security.IAccessTokenRevocationService.RevokeAllForUser"/>.
    /// </remarks>
    Task RemoveAdminAsync(Guid adminId, CancellationToken ct = default);

    /// <summary>
    /// Returns a paginated list of all registered users.
    /// </summary>
    /// <param name="page">One-based page number (clamped to at least 1).</param>
    /// <param name="pageSize">Page size (clamped between 1 and 50).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paged user list items.</returns>
    Task<PagedResult<UserListItemDto>> GetUsersAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Returns detailed information for a single user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>User detail DTO.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.NotFoundException">Thrown when the user does not exist.</exception>
    Task<UserDetailDto> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
}
