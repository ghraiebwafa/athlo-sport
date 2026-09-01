namespace Athlo.Shared.Security;

public interface IAccessTokenRevocationService
{
    void Revoke(string jti, DateTimeOffset expiresAt);
    bool IsRevoked(string jti);
    void RevokeAllForUser(Guid userId);
    bool IsRevokedForUser(Guid userId, DateTimeOffset issuedAt);
}
