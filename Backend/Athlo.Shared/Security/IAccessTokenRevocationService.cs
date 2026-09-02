namespace Athlo.Shared.Security;

/// <summary>
/// In-memory denylist for revoked JWT access tokens, backed by distributed cache.
/// Used to invalidate tokens immediately on logout or password change before natural expiry.
/// Checked on every authenticated request by the JWT bearer middleware.
/// </summary>
public interface IAccessTokenRevocationService
{
    /// <summary>
    /// Blacklists a single access token by its JWT ID (<c>jti</c>) until its natural expiry.
    /// </summary>
    /// <param name="jti">Unique token identifier from the <c>jti</c> claim.</param>
    /// <param name="expiresAt">Token expiration time; used to set cache entry TTL.</param>
    /// <remarks>No-op when <paramref name="jti"/> is empty or the token has already expired.</remarks>
    void Revoke(string jti, DateTimeOffset expiresAt);

    /// <summary>
    /// Returns whether a specific access token has been individually revoked by JTI.
    /// </summary>
    /// <param name="jti">Unique token identifier from the <c>jti</c> claim.</param>
    /// <returns><c>true</c> if the token is on the denylist; otherwise <c>false</c>.</returns>
    bool IsRevoked(string jti);

    /// <summary>
    /// Invalidates all access tokens issued for a user up to and including the current moment.
    /// </summary>
    /// <param name="userId">User whose sessions should be terminated.</param>
    /// <remarks>
    /// Typically invoked alongside refresh-token revocation on password change, password reset,
    /// or admin demotion. Entries expire after a fixed TTL (currently one day).
    /// </remarks>
    void RevokeAllForUser(Guid userId);

    /// <summary>
    /// Returns whether an access token was issued before a user-wide revocation event.
    /// </summary>
    /// <param name="userId">Token subject user identifier.</param>
    /// <param name="issuedAt">Token issue time from the <c>iat</c> claim.</param>
    /// <returns>
    /// <c>true</c> when the token was issued at or before the last
    /// <see cref="RevokeAllForUser"/> call for this user; otherwise <c>false</c>.
    /// </returns>
    bool IsRevokedForUser(Guid userId, DateTimeOffset issuedAt);
}
