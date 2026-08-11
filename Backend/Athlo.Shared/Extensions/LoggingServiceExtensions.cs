using Serilog;
using Serilog.Events;

namespace Athlo.Shared.Extensions;

public static class LoggingServiceExtensions
{
    public static WebApplicationBuilder AddAthloLogging(this WebApplicationBuilder builder, string serviceName)
    {
        var configuration = builder.Configuration;
        var logConfig = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", serviceName)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}");

        var filePath = configuration["Serilog:FilePath"];
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            logConfig.WriteTo.File(
                filePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: configuration.GetValue("Serilog:RetainedFileCountLimit", 14),
                shared: true,
                outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}");
        }

        var seqUrl = configuration["Serilog:SeqUrl"] ?? configuration["SEQ_URL"];
        if (!string.IsNullOrWhiteSpace(seqUrl))
        {
            var apiKey = configuration["Serilog:SeqApiKey"];
            logConfig.WriteTo.Seq(seqUrl, apiKey: string.IsNullOrWhiteSpace(apiKey) ? null : apiKey);
        }

        Log.Logger = logConfig.CreateLogger();
        builder.Host.UseSerilog();
        return builder;
    }
}
