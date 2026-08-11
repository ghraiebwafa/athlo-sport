using Athlo.AuthService.Background;
using Athlo.AuthService.Services;
using Athlo.AuthService.Validators;
using Athlo.Database;
using Athlo.Database.DbContexts;
using AuthServiceImpl = Athlo.AuthService.Services.AuthService;
using Athlo.Database.Seed;
using Athlo.Repositories;
using Athlo.Shared.Extensions;
using Athlo.Shared.Security;
using Athlo.Shared.Settings;
using Microsoft.Extensions.Options;
using Serilog;

namespace Athlo.AuthService;

public class Startup(IConfiguration configuration, IWebHostEnvironment environment)
{
    public IConfiguration Configuration { get; } = configuration;
    public IWebHostEnvironment Environment { get; } = environment;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAthloApiDefaults(Configuration);
        services.AddAthloSwagger("Athlo Auth API", Environment);
        services.AddAthloCors(Configuration);
        services.AddAthloForwardedHeaders(Configuration);
        services.AddAthloDistributedCache(Configuration);
        if (!Environment.IsEnvironment("Testing"))
            services.AddAthloRateLimiting(Configuration);
        services.AddSingleton<LoginAttemptLimiter>();
        services.AddAthloTokenCleanup();
        services.AddAthloDatabase(Configuration);
        services.AddAthloRepositories();
        services.AddAthloJwtAuthentication(Configuration);
        services.AddAthloEmail(Configuration, Environment);
        services.AddAthloFluentValidation<RegisterRequestValidator>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthServiceImpl>();
        services.AddScoped<IAdminService, AdminService>();
    }

    public async Task InitializeAsync(WebApplication app)
    {
        if (Environment.IsEnvironment("Testing"))
            return;

        try
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AthloDbContext>();
            var superAdminSettings = scope.ServiceProvider.GetRequiredService<IOptions<SuperAdminSettings>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Startup>>();
            await DataSeeder.SeedAsync(context, superAdminSettings, logger);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Database migration/seed failed");
            throw;
        }
    }

    public void Configure(WebApplication app)
    {
        app.UseAthloForwardedHeaders();
        app.UseAthloSwagger("Athlo Auth API");
        if (!Environment.IsEnvironment("Testing"))
            app.UseRateLimiter();
        app.UseAthloDefaults();
        app.MapAthloHealthChecks();
        app.MapControllers();
    }
}
