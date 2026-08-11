using Athlo.Database;
using Athlo.Database.DbContexts;
using Athlo.ManagementService.Background;
using Athlo.ManagementService.Services;
using Athlo.ManagementService.Validators;
using Athlo.Repositories;
using Athlo.Shared.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Athlo.ManagementService;

public class Startup(IConfiguration configuration, IWebHostEnvironment environment)
{
    public IConfiguration Configuration { get; } = configuration;
    public IWebHostEnvironment Environment { get; } = environment;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAthloApiDefaults(Configuration);
        services.AddAthloSwagger("Athlo Management API", Environment);
        services.AddAthloCors(Configuration);
        services.AddAthloForwardedHeaders(Configuration);
        services.AddAthloDistributedCache(Configuration);
        if (!Environment.IsEnvironment("Testing"))
            services.AddAthloRateLimiting(Configuration);
        services.AddAthloDatabase(Configuration);
        services.AddAthloRepositories();
        services.AddAthloJwtAuthentication(Configuration);
        services.AddAthloFluentValidation<CreateProgramRequestValidator>();
        services.AddScoped<IProgramService, ProgramService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IExerciseService, ExerciseService>();
        services.AddScoped<IWorkoutService, WorkoutService>();
        services.AddScoped<IProgressService, ProgressService>();
        services.AddScoped<IAdminStatsService, AdminStatsService>();
        if (!Environment.IsEnvironment("Testing"))
            services.AddAthloStaleWorkoutCleanup();
    }

    public async Task InitializeAsync(WebApplication app)
    {
        if (Environment.IsEnvironment("Testing"))
            return;

        try
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AthloDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Startup>>();
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied.");
        }
        catch (Exception ex)
        {
            Serilog.Log.Fatal(ex, "Database migration failed");
            throw;
        }
    }

    public void Configure(WebApplication app)
    {
        app.UseAthloForwardedHeaders();
        app.UseAthloSwagger("Athlo Management API");
        if (!Environment.IsEnvironment("Testing"))
            app.UseRateLimiter();
        app.UseAthloDefaults();
        app.MapAthloHealthChecks();
        app.MapControllers();
    }
}
