using System.Threading.RateLimiting;
using Athlo.Shared.Helpers;
using Athlo.Shared.Models;
using Athlo.Shared.RateLimiting;
using StackExchange.Redis;

namespace Athlo.Shared.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddAthloRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConfig = CachingServiceExtensions.GetRedisConfiguration(configuration);
        if (!string.IsNullOrWhiteSpace(redisConfig))
        {
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisConfig));
        }

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, _) =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                var payload = ApiErrorFactory.Create(
                    ApiErrorCodes.RateLimited,
                    "Too many requests. Please try again later.",
                    traceId: context.HttpContext.TraceIdentifier);
                await context.HttpContext.Response.WriteAsJsonAsync(payload);
            };

            var failOpen = configuration.GetValue("RateLimiting:FailOpen", false);

            options.AddPolicy("auth", httpContext =>
                CreatePartition(httpContext, "auth", permitLimit: 10, failOpen));

            options.AddPolicy("api", httpContext =>
                CreatePartition(httpContext, "api", permitLimit: 100, failOpen));
        });

        return services;
    }

    private static RateLimitPartition<string> CreatePartition(
        HttpContext httpContext,
        string policy,
        int permitLimit,
        bool failOpen)
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = $"{policy}:{ip}";
        var window = TimeSpan.FromMinutes(1);
        var redis = httpContext.RequestServices.GetService<IConnectionMultiplexer>();

        if (redis is not null)
        {
            return RateLimitPartition.Get(
                key,
                partitionKey => new RedisFixedWindowRateLimiter(
                    redis, partitionKey, permitLimit, window, failOpen));
        }

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: key,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = window,
                QueueLimit = 0
            });
    }
}
