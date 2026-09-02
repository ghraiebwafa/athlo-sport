using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Athlo.Shared.Extensions;

/// <summary>
/// Swagger/OpenAPI setup for local development.
/// </summary>
public static class SwaggerServiceExtensions
{
    /// <summary>
    /// Registers Swagger with JWT bearer auth. Only enabled in Development.
    /// XML comments from the supplied assemblies are included when present.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="title">API title shown in Swagger UI.</param>
    /// <param name="environment">Host environment.</param>
    /// <param name="xmlCommentAssemblies">Assemblies whose generated XML docs should appear in Swagger.</param>
    public static IServiceCollection AddAthloSwagger(
        this IServiceCollection services,
        string title,
        IHostEnvironment environment,
        params Assembly[] xmlCommentAssemblies)
    {
        if (!environment.IsDevelopment())
            return services;

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = title,
                Version = "v1",
                Description = "Athlo REST API. See Backend/Docs/DEVELOPERS.md for architecture and conventions."
            });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme."
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });

            foreach (var assembly in xmlCommentAssemblies)
                IncludeXmlCommentsIfPresent(options, assembly);
            IncludeXmlCommentsIfPresent(options, typeof(SwaggerServiceExtensions).Assembly);
        });
        return services;
    }

    /// <summary>Enables Swagger UI middleware in Development.</summary>
    public static WebApplication UseAthloSwagger(this WebApplication app, string routePrefix)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        app.UseSwagger();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", routePrefix));
        return app;
    }

    private static void IncludeXmlCommentsIfPresent(
        Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options,
        Assembly assembly)
    {
        var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    }
}
