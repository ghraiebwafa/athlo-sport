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
    }
}
