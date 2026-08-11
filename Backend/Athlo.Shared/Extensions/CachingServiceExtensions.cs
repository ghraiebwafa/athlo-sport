namespace Athlo.Shared.Extensions;

public static class CachingServiceExtensions
{
    /// <summary>
    /// Uses Redis when ConnectionStrings:Redis (or Redis:Configuration) is set;
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

    public static string? GetRedisConfiguration(IConfiguration configuration) =>
        configuration.GetConnectionString("Redis")
        ?? configuration["Redis:Configuration"];

    public static bool HasRedis(IConfiguration configuration) =>
        !string.IsNullOrWhiteSpace(GetRedisConfiguration(configuration));
}
