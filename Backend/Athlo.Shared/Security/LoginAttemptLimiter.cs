using Microsoft.Extensions.Caching.Memory;

namespace Athlo.Shared.Security;

/// <summary>Throttles repeated failed login attempts per email address.</summary>
public class LoginAttemptLimiter(IMemoryCache cache)
{
    private const int MaxFailures = 5;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

    public void EnsureNotBlocked(string email)
    {
        var key = CacheKey(email);
        if (cache.TryGetValue<int>(key, out var failures) && failures >= MaxFailures)
            throw new Exceptions.UnauthorizedException("Too many failed login attempts. Try again later.");
    }

    public void RecordFailure(string email)
    {
        var key = CacheKey(email);
        var failures = cache.TryGetValue<int>(key, out var current) ? current + 1 : 1;
        cache.Set(key, failures, Window);
    }

    public void Reset(string email) => cache.Remove(CacheKey(email));

    private static string CacheKey(string email) => $"login_fail:{email.Trim().ToLowerInvariant()}";
}
