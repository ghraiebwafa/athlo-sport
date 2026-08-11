using System.Threading.RateLimiting;
using StackExchange.Redis;

namespace Athlo.Shared.RateLimiting;

/// <summary>Fixed-window rate limiter backed by Redis INCR/EXPIRE (multi-instance safe).</summary>
public sealed class RedisFixedWindowRateLimiter : RateLimiter
{
    private readonly IConnectionMultiplexer _redis;
    private readonly string _key;
    private readonly int _permitLimit;
    private readonly TimeSpan _window;
    private int _disposed;

    public RedisFixedWindowRateLimiter(
        IConnectionMultiplexer redis,
        string partitionKey,
        int permitLimit,
        TimeSpan window)
    {
        _redis = redis;
        _key = $"rl:{partitionKey}";
        _permitLimit = permitLimit;
        _window = window;
    }

    public override TimeSpan? IdleDuration => null;

    public override RateLimiterStatistics? GetStatistics() => null;

    protected override RateLimitLease AttemptAcquireCore(int permitCount)
    {
        if (permitCount != 1)
            return new RedisRateLimitLease(false);

        try
        {
            var db = _redis.GetDatabase();
            var count = db.StringIncrement(_key);
            if (count == 1)
                db.KeyExpire(_key, _window);

            return new RedisRateLimitLease(count <= _permitLimit);
        }
        catch
        {
            // Fail open on Redis errors so the API stays available.
            return new RedisRateLimitLease(true);
        }
    }

    protected override ValueTask<RateLimitLease> AcquireAsyncCore(
        int permitCount,
        CancellationToken cancellationToken)
    {
        return new ValueTask<RateLimitLease>(AttemptAcquireCore(permitCount));
    }

    protected override void Dispose(bool disposing)
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;
        base.Dispose(disposing);
    }

    private sealed class RedisRateLimitLease(bool isAcquired) : RateLimitLease
    {
        public override bool IsAcquired { get; } = isAcquired;

        public override IEnumerable<string> MetadataNames => [];

        public override bool TryGetMetadata(string metadataName, out object? metadata)
        {
            metadata = null;
            return false;
        }
    }
}
