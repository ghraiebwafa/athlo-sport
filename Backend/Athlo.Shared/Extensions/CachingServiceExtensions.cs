using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Athlo.Shared.Extensions;

/// <summary>
/// Registers distributed cache (Redis when configured, otherwise in-memory per process).
/// </summary>
public static class CachingServiceExtensions
{
    /// <summary>
    /// Uses Redis when <c>ConnectionStrings:Redis</c> (or <c>Redis:Configuration</c>) is set;
    /// otherwise falls back to in-process distributed memory cache.
    /// </summary>
    public static IServiceCollection AddAthloDistributedCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redis = GetRedisConfiguration(configuration);
        if (!string.IsNullOrWhiteSpace(redis))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redis;
                options.InstanceName = configuration["Redis:InstanceName"] ?? "athlo:";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        return services;
    }

    /// <summary>
    /// Logs a warning when Redis is not configured. In-memory distributed cache is per process,
    /// so JWT revocation and login lockout do not propagate across multiple API instances.
    /// </summary>
    public static void WarnIfUsingInMemoryDistributedCache(
        IConfiguration configuration,
        Microsoft.Extensions.Logging.ILogger logger,
        string serviceName)
    {
        if (HasRedis(configuration))
            return;

        logger.LogWarning(
            "{Service} is using in-memory distributed cache. Configure ConnectionStrings:Redis for shared JWT revocation and login lockout across instances.",
            serviceName);
    }

    public static string? GetRedisConfiguration(IConfiguration configuration) =>
        configuration.GetConnectionString("Redis")
        ?? configuration["Redis:Configuration"];

    public static bool HasRedis(IConfiguration configuration) =>
        !string.IsNullOrWhiteSpace(GetRedisConfiguration(configuration));
}
