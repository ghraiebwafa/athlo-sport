using Athlo.Shared.Exceptions;
using Athlo.Shared.Security;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Athlo.Tests;

public class LoginAttemptLimiterTests
{
    [Fact]
    public void RecordFailure_IsSafeUnderConcurrentCalls()
    {
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var limiter = new LoginAttemptLimiter(cache, []);
        var email = $"concurrent_{Guid.NewGuid():N}@test.local";

        Parallel.For(0, 5, _ => limiter.RecordFailure(email));

        var blocked = Assert.Throws<UnauthorizedException>(() => limiter.EnsureNotBlocked(email));
        Assert.Contains("Too many failed login attempts", blocked.Message);
    }

    [Fact]
    public void Reset_ClearsFailures()
    {
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var limiter = new LoginAttemptLimiter(cache, []);
        var email = $"reset_{Guid.NewGuid():N}@test.local";

        for (var i = 0; i < 5; i++)
            limiter.RecordFailure(email);

        Assert.Throws<UnauthorizedException>(() => limiter.EnsureNotBlocked(email));
        limiter.Reset(email);
        limiter.EnsureNotBlocked(email);
    }
}
