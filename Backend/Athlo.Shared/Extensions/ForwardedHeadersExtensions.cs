using Microsoft.AspNetCore.HttpOverrides;

namespace Athlo.Shared.Extensions;

public static class ForwardedHeadersExtensions
{
    /// <summary>
    /// Trust X-Forwarded-For / X-Forwarded-Proto from reverse proxies so
    /// RemoteIpAddress, IsHttps, and HSTS work correctly behind TLS termination.
    /// </summary>
    public static IServiceCollection AddAthloForwardedHeaders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            options.ForwardLimit = configuration.GetValue("ForwardedHeaders:ForwardLimit", 1);

            // Docker / private networks: KnownProxies is empty by default and blocks
            // forwarded headers. Clear when explicitly trusting the compose network.
            var trustAll = configuration.GetValue("ForwardedHeaders:TrustAllProxies", false);
            if (trustAll)
            {
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            }

            var knownProxies = configuration["ForwardedHeaders:KnownProxies"]?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (knownProxies is { Length: > 0 })
            {
                foreach (var proxy in knownProxies)
                {
                    if (System.Net.IPAddress.TryParse(proxy, out var address))
                        options.KnownProxies.Add(address);
                }
            }
        });

        return services;
    }

    public static WebApplication UseAthloForwardedHeaders(this WebApplication app)
    {
        app.UseForwardedHeaders();
        return app;
    }
}
