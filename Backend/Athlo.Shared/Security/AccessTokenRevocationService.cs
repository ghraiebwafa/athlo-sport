using Microsoft.Extensions.Caching.Distributed;

namespace Athlo.Shared.Security;

/// <summary>Denylist for revoked access-token JTIs (logout) until natural expiry.</summary>
public class AccessTokenRevocationService(IDistributedCache cache) : IAccessTokenRevocationService
{
    public void Revoke(string jti, DateTimeOffset expiresAt)
    {
        if (string.IsNullOrWhiteSpace(jti))
            return;

        var ttl = expiresAt - DateTimeOffset.UtcNow;
        if (ttl <= TimeSpan.Zero)
            return;

        cache.SetString(
            CacheKey(jti),
            "1",
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl });
    }

    public bool IsRevoked(string jti) =>
        !string.IsNullOrWhiteSpace(jti) && cache.GetString(CacheKey(jti)) is not null;

    private static string CacheKey(string jti) => $"jwt_revoked:{jti}";
}
