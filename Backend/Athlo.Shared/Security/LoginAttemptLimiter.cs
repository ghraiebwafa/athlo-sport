using Microsoft.Extensions.Caching.Distributed;

namespace Athlo.Shared.Security;

/// <summary>Throttles repeated failed login attempts per email address (distributed-cache safe).</summary>
public class LoginAttemptLimiter(IDistributedCache cache)
{
    private const int MaxFailures = 5;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

    public void EnsureNotBlocked(string email)
    {
        var key = CacheKey(email);
        var raw = cache.GetString(key);
        if (raw is not null
            && int.TryParse(raw, out var failures)
            && failures >= MaxFailures)
        {
            throw new Exceptions.UnauthorizedException("Too many failed login attempts. Try again later.");
        }
    }

    public void RecordFailure(string email)
    {
        var key = CacheKey(email);
        var raw = cache.GetString(key);
        var failures = raw is not null && int.TryParse(raw, out var current) ? current + 1 : 1;
        cache.SetString(
            key,
            failures.ToString(),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Window });
    }

    public void Reset(string email) => cache.Remove(CacheKey(email));

    private static string CacheKey(string email) => $"login_fail:{email.Trim().ToLowerInvariant()}";
}
