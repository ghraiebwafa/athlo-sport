using Microsoft.Extensions.Caching.Distributed;

namespace Athlo.Shared.Security;

/// <summary>Denylist for revoked access-token JTIs (logout) until natural expiry.</summary>
public class AccessTokenRevocationService(IDistributedCache cache) : IAccessTokenRevocationService
{
    private static readonly TimeSpan UserRevocationTtl = TimeSpan.FromDays(1);

    public void Revoke(string jti, DateTimeOffset expiresAt)
    {
        if (string.IsNullOrWhiteSpace(jti))
            return;

        var ttl = expiresAt - DateTimeOffset.UtcNow;
        if (ttl <= TimeSpan.Zero)
            return;

        cache.SetString(
            JtiCacheKey(jti),
            "1",
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl });
    }

    public bool IsRevoked(string jti) =>
        !string.IsNullOrWhiteSpace(jti) && cache.GetString(JtiCacheKey(jti)) is not null;

    public void RevokeAllForUser(Guid userId)
    {
        var revokedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        cache.SetString(
            UserCacheKey(userId),
            revokedAt.ToString(),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = UserRevocationTtl });
    }

    public bool IsRevokedForUser(Guid userId, DateTimeOffset issuedAt)
    {
        var raw = cache.GetString(UserCacheKey(userId));
        if (raw is null || !long.TryParse(raw, out var revokedUnix))
            return false;

        var issuedUnix = issuedAt.ToUniversalTime().ToUnixTimeSeconds();
        return issuedUnix <= revokedUnix;
    }

    private static string JtiCacheKey(string jti) => $"jwt_revoked:{jti}";

    private static string UserCacheKey(Guid userId) => $"jwt_user_revoked:{userId:N}";
}
