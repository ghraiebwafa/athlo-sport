using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Athlo.Shared.Authorization;
using Athlo.Shared.Security;
using Athlo.Shared.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace Athlo.Shared.Extensions;

public static class AuthenticationServiceExtensions
{
    public static IServiceCollection AddAthloJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<IAccessTokenRevocationService, AccessTokenRevocationService>();

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

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var revocation = context.HttpContext.RequestServices
                            .GetRequiredService<IAccessTokenRevocationService>();

                        var jti = context.Principal?.FindFirst(JwtClaimNames.Jti)?.Value;
                        if (!string.IsNullOrEmpty(jti) && revocation.IsRevoked(jti))
                        {
                            context.Fail("Access token has been revoked.");
                            return Task.CompletedTask;
                        }

                        var userIdRaw = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? context.Principal?.FindFirstValue(JwtClaimNames.Sub);
                        var issuedAt = GetTokenIssuedAt(context);
                        if (userIdRaw is not null
                            && Guid.TryParse(userIdRaw, out var userId)
                            && issuedAt is not null
                            && revocation.IsRevokedForUser(userId, issuedAt.Value))
                        {
                            context.Fail("Access tokens for this user have been revoked.");
                        }

                        return Task.CompletedTask;
                    }
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

    private static DateTimeOffset? GetTokenIssuedAt(TokenValidatedContext context)
    {
        var iatRaw = context.Principal?.FindFirstValue(JwtClaimNames.Iat);
        if (iatRaw is not null && long.TryParse(iatRaw, out var iatUnix))
            return DateTimeOffset.FromUnixTimeSeconds(iatUnix);

        if (context.SecurityToken is SecurityToken securityToken && securityToken.ValidFrom > DateTime.MinValue)
            return new DateTimeOffset(DateTime.SpecifyKind(securityToken.ValidFrom, DateTimeKind.Utc));

        if (context.SecurityToken is JwtSecurityToken jwt && jwt.IssuedAt != default)
            return new DateTimeOffset(DateTime.SpecifyKind(jwt.IssuedAt, DateTimeKind.Utc));

        if (context.SecurityToken is JsonWebToken jsonWebToken
            && jsonWebToken.TryGetPayloadValue<long>(JwtClaimNames.Iat, out var jsonIatUnix))
            return DateTimeOffset.FromUnixTimeSeconds(jsonIatUnix);

        return null;
    }
}
