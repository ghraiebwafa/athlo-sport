using Athlo.Database;
using Athlo.ManagementService.Services;
using Athlo.ManagementService.Validators;
using Athlo.Repositories;
using Athlo.Shared.Extensions;

namespace Athlo.ManagementService;

public class Startup(IConfiguration configuration, IWebHostEnvironment environment)
{
    public IConfiguration Configuration { get; } = configuration;
    public IWebHostEnvironment Environment { get; } = environment;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAthloApiDefaults(Configuration);
        services.AddAthloSwagger("Athlo Management API");
        services.AddAthloCors(Configuration);
        services.AddAthloHealthChecks();
        services.AddAthloRateLimiting();
        services.AddAthloDatabase(Configuration);
        services.AddAthloRepositories();
        services.AddAthloJwtAuthentication(Configuration);
        services.AddAthloFluentValidation<CreateProgramRequestValidator>();
        services.AddScoped<IProgramService, ProgramService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IExerciseService, ExerciseService>();
        services.AddScoped<IWorkoutService, WorkoutService>();
        services.AddScoped<IProgressService, ProgressService>();
    }

    public void Configure(WebApplication app)
    {
        app.UseAthloSwagger("Athlo Management API");
        app.UseRateLimiter();
        app.UseAthloDefaults();
        app.MapAthloHealthChecks();
        app.MapControllers();
    }
}
