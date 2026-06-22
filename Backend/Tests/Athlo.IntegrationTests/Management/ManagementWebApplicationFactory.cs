using Athlo.Database.DbContexts;
using Athlo.Models.Entities;
using Athlo.Shared.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Athlo.IntegrationTests.Management;

public class ManagementWebApplicationFactory : WebApplicationFactory<Athlo.ManagementService.Program>
{
    private readonly string _databaseName = $"AthloMgmtTest_{Guid.NewGuid()}";
    private int _initialized;

    public ManagementWebApplicationFactory() => TestEnvironment.Apply();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var settings = new Dictionary<string, string?>(TestConfiguration.Values)
            {
                ["ATHLO_INMEMORY_DB"] = _databaseName
            };
            config.AddInMemoryCollection(settings);
        });
    }

    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);

        if (Interlocked.CompareExchange(ref _initialized, 1, 0) != 0)
            return;
        InMemoryDbHelper.EnsureCreated(Services);
        SeedCatalog(Services);
    }

    private static void SeedCatalog(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AthloDbContext>();

        var categoryId = Guid.Parse("11111111-1111-1111-1111-111111111101");
        if (context.Categories.Any(c => c.Id == categoryId))
            return;

        var exerciseId = Guid.Parse("33333333-3333-3333-3333-333333333301");
        var programId = Guid.Parse("22222222-2222-2222-2222-222222222201");

        context.Categories.Add(new Category
        {
            Id = categoryId,
            Name = "Strength",
            Slug = "strength",
            Icon = "dumbbell"
        });

        context.Exercises.Add(new Exercise
        {
            Id = exerciseId,
            Name = "Push Ups"
        });

        context.WorkoutPrograms.Add(new WorkoutProgram
        {
            Id = programId,
            Name = "Test Program",
            Description = "Integration test program",
            DurationMinutes = 30,
            Difficulty = WorkoutDifficulty.Beginner,
            EstimatedCalories = 200,
            CategoryId = categoryId,
            IsFeatured = true
        });

        context.ProgramExercises.Add(new ProgramExercise
        {
            Id = Guid.NewGuid(),
            ProgramId = programId,
            ExerciseId = exerciseId,
            OrderIndex = 1,
            Sets = 3,
            Reps = 10
        });

        context.SaveChanges();
    }
}
