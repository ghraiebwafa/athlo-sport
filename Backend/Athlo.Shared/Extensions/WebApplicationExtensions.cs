using Athlo.Shared.Middleware;

namespace Athlo.Shared.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseAthloDefaults(this WebApplication app)
    {
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseSerilogRequestLogging();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    public static IServiceCollection AddAthloCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration["Cors:AllowedOrigins"]?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? ["http://localhost:8081", "http://localhost:19006"];

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.WithOrigins(origins)
                      .WithHeaders("Content-Type", "Authorization", "Accept")
                      .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS"));
        });

        return services;
    }

    public static WebApplication MapAthloHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        return app;
    }
}
