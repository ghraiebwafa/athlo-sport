using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Sentry.AspNetCore;

namespace Athlo.Shared.Extensions;

/// <summary>
/// Optional Sentry crash reporting. Enabled when <c>Sentry:Dsn</c> (or <c>SENTRY_DSN</c>) is set.
/// </summary>
public static class SentryServiceExtensions
{
    public static WebApplicationBuilder AddAthloSentry(this WebApplicationBuilder builder, string serviceName)
    {
        var dsn = builder.Configuration["Sentry:Dsn"]
            ?? builder.Configuration["SENTRY_DSN"];

        if (string.IsNullOrWhiteSpace(dsn) || builder.Environment.IsEnvironment("Testing"))
            return builder;

        builder.WebHost.UseSentry(options =>
        {
            options.Dsn = dsn;
            options.Environment = builder.Environment.EnvironmentName;
            options.Release = builder.Configuration["Sentry:Release"];
            options.TracesSampleRate = builder.Configuration.GetValue("Sentry:TracesSampleRate", 0.1);
            options.SendDefaultPii = false;
            options.DefaultTags["service"] = serviceName;
        });

        return builder;
    }
}
