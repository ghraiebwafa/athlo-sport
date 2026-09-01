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
}
