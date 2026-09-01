using Athlo.Shared.Security;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Athlo.Tests;

public class AccessTokenRevocationServiceTests
{
    [Fact]
    public void Revoke_MarksTokenAsRevokedUntilExpiry()
    {
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var service = new AccessTokenRevocationService(cache);
        var jti = Guid.NewGuid().ToString();

        service.Revoke(jti, DateTimeOffset.UtcNow.AddMinutes(5));

        Assert.True(service.IsRevoked(jti));
    }

    [Fact]
    public void IsRevoked_ReturnsFalseForUnknownJti()
    {
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var service = new AccessTokenRevocationService(cache);

        Assert.False(service.IsRevoked(Guid.NewGuid().ToString()));
    }

    [Fact]
    public void RevokeAllForUser_InvalidatesTokensIssuedBeforeRevocation()
    {
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var service = new AccessTokenRevocationService(cache);
        var userId = Guid.NewGuid();
        var issuedBefore = DateTimeOffset.UtcNow.AddMinutes(-5);

        service.RevokeAllForUser(userId);

        Assert.True(service.IsRevokedForUser(userId, issuedBefore));
        Assert.False(service.IsRevokedForUser(userId, DateTimeOffset.UtcNow.AddMinutes(1)));
    }

    [Fact]
    public void RevokeAllForUser_RevokesTokenIssuedInSameSecond()
    {
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var service = new AccessTokenRevocationService(cache);
        var userId = Guid.NewGuid();
        var issuedAt = DateTimeOffset.UtcNow;

        service.RevokeAllForUser(userId);

        Assert.True(service.IsRevokedForUser(userId, issuedAt));
    }
}
