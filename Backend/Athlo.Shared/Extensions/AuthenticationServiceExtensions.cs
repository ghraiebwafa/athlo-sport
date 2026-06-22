using System.Text;
using Athlo.Shared.Authorization;
using Athlo.Shared.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Athlo.Shared.Extensions;

public static class AuthenticationServiceExtensions
{
    public static IServiceCollection AddAthloJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings not configured.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AthloPolicies.SuperAdminOnly, policy =>
                policy.RequireRole(AthloRoles.SuperAdmin));

            options.AddPolicy(AthloPolicies.AdminOrSuperAdmin, policy =>
                policy.RequireRole(AthloRoles.Admin, AthloRoles.SuperAdmin));
        });

        return services;
    }
}
