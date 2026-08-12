using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Athlo.Shared.Security;

/// <summary>Throttles repeated failed login attempts per email address (distributed-cache safe).</summary>
public class LoginAttemptLimiter(
    IDistributedCache cache,
    IEnumerable<IConnectionMultiplexer> redisConnections)
{
    private const int MaxFailures = 5;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);
    private static readonly ConcurrentDictionary<string, object> KeyLocks = new();

    private readonly IConnectionMultiplexer? _redis = redisConnections.FirstOrDefault();

    public void EnsureNotBlocked(string email)
    {
        var key = CacheKey(email);
        var failures = GetFailureCount(key);
        if (failures >= MaxFailures)
            throw new Exceptions.UnauthorizedException("Too many failed login attempts. Try again later.");
    }

    public void RecordFailure(string email)
    {
        var key = CacheKey(email);

        if (_redis is not null)
        {
            var db = _redis.GetDatabase();
            var count = db.StringIncrement(key);
            if (count == 1)
                db.KeyExpire(key, Window);
            return;
        }

        // In-process cache: serialize read-modify-write per key.
        lock (KeyLocks.GetOrAdd(key, static _ => new object()))
        {
            var raw = cache.GetString(key);
            var failures = raw is not null && int.TryParse(raw, out var current) ? current + 1 : 1;
            cache.SetString(
                key,
                failures.ToString(),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Window });
        }
    }

    public void Reset(string email)
    {
        var key = CacheKey(email);
        if (_redis is not null)
        {
            _redis.GetDatabase().KeyDelete(key);
            return;
        }

        lock (KeyLocks.GetOrAdd(key, static _ => new object()))
            cache.Remove(key);
    }

    private long GetFailureCount(string key)
    {
        if (_redis is not null)
        {
            var value = _redis.GetDatabase().StringGet(key);
            return value.HasValue && long.TryParse(value, out var count) ? count : 0;
        }

        lock (KeyLocks.GetOrAdd(key, static _ => new object()))
        {
            var raw = cache.GetString(key);
            return raw is not null && long.TryParse(raw, out var failures) ? failures : 0;
        }
    }

    private static string CacheKey(string email) => $"login_fail:{email.Trim().ToLowerInvariant()}";
}
