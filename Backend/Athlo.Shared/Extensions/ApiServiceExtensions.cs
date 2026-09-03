using System.Text.Json.Serialization;
using Athlo.Shared.Configuration;
using Athlo.Shared.Filters;
using Athlo.Shared.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Athlo.Shared.Extensions;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddAthloApiDefaults(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers(options => options.Filters.Add<ValidationFilter>())
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<SuperAdminSettings>(configuration.GetSection(SuperAdminSettings.SectionName));
        services.Configure<MediaSettings>(configuration.GetSection(MediaSettings.SectionName));
        return services;
    }

    public static void ValidateAthloConfiguration(this IConfiguration configuration)
    {
        EnvConfiguration.ValidateRequiredSettings(configuration);

        var jwtSecret = configuration["Jwt:Secret"];
        if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
            throw new InvalidOperationException("Jwt:Secret must be at least 32 characters.");

        var superAdminPassword = configuration["SuperAdmin:Password"];
        if (string.IsNullOrWhiteSpace(superAdminPassword) || superAdminPassword.Length < 12)
            throw new InvalidOperationException("SuperAdmin:Password must be at least 12 characters.");

        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
        var isDevOrTest = string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase)
            || string.Equals(environment, "Testing", StringComparison.OrdinalIgnoreCase);
        var smtpHost = configuration["Smtp:Host"];
        if (!isDevOrTest && string.IsNullOrWhiteSpace(smtpHost))
            throw new InvalidOperationException(
                "Smtp:Host must be configured in production so password reset emails can be sent.");

        var allowedHosts = configuration["AllowedHosts"];
        if (!isDevOrTest && allowedHosts == "*")
            throw new InvalidOperationException(
                "AllowedHosts must not be '*' in production. Set it to your domain(s) separated by ';'.");
    }
}
