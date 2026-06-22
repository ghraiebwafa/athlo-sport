using Athlo.Database.DbContexts;
using Athlo.Models.Entities;
using Athlo.Shared.Enums;
using Athlo.Shared.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Athlo.Database.Seed;

public static class DataSeeder
{
    private static readonly Guid StrengthCategoryId = Guid.Parse("11111111-1111-1111-1111-111111111101");
    private static readonly Guid HiitCategoryId = Guid.Parse("11111111-1111-1111-1111-111111111102");
    private static readonly Guid CardioCategoryId = Guid.Parse("11111111-1111-1111-1111-111111111103");
    private static readonly Guid YogaCategoryId = Guid.Parse("11111111-1111-1111-1111-111111111104");

    private static readonly Guid UpperBodyProgramId = Guid.Parse("22222222-2222-2222-2222-222222222201");
    private static readonly Guid SummerShredProgramId = Guid.Parse("22222222-2222-2222-2222-222222222202");
    private static readonly Guid FullBodyHiitProgramId = Guid.Parse("22222222-2222-2222-2222-222222222203");

    public static async Task SeedAsync(
        AthloDbContext context,
        IOptions<SuperAdminSettings> superAdminSettings,
        ILogger logger)
    {
        await context.Database.MigrateAsync();
        await SuperAdminSeeder.EnsureAsync(context, superAdminSettings, logger);

        if (await context.Categories.AnyAsync())
        {
            logger.LogInformation("Database already seeded.");
            return;
        }

        var categories = new[]
        {
            new Category { Id = StrengthCategoryId, Name = "Strength", Slug = "strength", Icon = "dumbbell" },
            new Category { Id = HiitCategoryId, Name = "HIIT", Slug = "hiit", Icon = "flame" },
            new Category { Id = CardioCategoryId, Name = "Cardio", Slug = "cardio", Icon = "running" },
            new Category { Id = YogaCategoryId, Name = "Yoga", Slug = "yoga", Icon = "lotus" }
        };

        var exercises = CreateExercises();
        var programs = CreatePrograms();
        var programExercises = CreateProgramExercises(exercises);

        context.Categories.AddRange(categories);
        context.Exercises.AddRange(exercises);
        context.WorkoutPrograms.AddRange(programs);
        context.ProgramExercises.AddRange(programExercises);

        await context.SaveChangesAsync();
        logger.LogInformation("Database seeded successfully.");
    }

    private static List<Exercise> CreateExercises() =>
    [
        new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333301"), Name = "Push Ups" },
        new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333302"), Name = "Dumbbell Press" },
        new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333303"), Name = "Shoulder Press" },
        new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333304"), Name = "Bent Over Rows" },
        new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333305"), Name = "Plank" },
        new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333306"), Name = "Bicep Curls" },
        new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333307"), Name = "Tricep Dips" },
        new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333308"), Name = "Lateral Raises" },
        new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333309"), Name = "Battle Ropes" },
        new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333310"), Name = "Burpees" },
        new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333311"), Name = "Jump Squats" },
        new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333312"), Name = "Mountain Climbers" }
    ];

    private static List<WorkoutProgram> CreatePrograms() =>
    [
        new()
        {
            Id = UpperBodyProgramId,
            Name = "Upper Body Strength",
            Description = "Build upper body strength with a balanced workout targeting chest, shoulders, arms and core.",
            DurationMinutes = 42,
            Difficulty = WorkoutDifficulty.Intermediate,
            EstimatedCalories = 320,
            CategoryId = StrengthCategoryId,
            IsFeatured = false
        },
        new()
        {
            Id = SummerShredProgramId,
            Name = "Summer Shred Challenge",
            Description = "A 4-week intermediate program designed to burn fat and build lean muscle.",
            DurationMinutes = 45,
            Difficulty = WorkoutDifficulty.Intermediate,
            EstimatedCalories = 380,
            CategoryId = HiitCategoryId,
            IsFeatured = true
        },
        new()
        {
            Id = FullBodyHiitProgramId,
            Name = "Full Body HIIT",
            Description = "High-intensity interval training to maximize calorie burn in minimal time.",
            DurationMinutes = 30,
            Difficulty = WorkoutDifficulty.Advanced,
            EstimatedCalories = 400,
            CategoryId = HiitCategoryId,
            IsFeatured = false
        }
    ];

    private static List<ProgramExercise> CreateProgramExercises(List<Exercise> exercises)
    {
        var byName = exercises.ToDictionary(e => e.Name, e => e.Id);

        return
        [
            PE(UpperBodyProgramId, byName["Push Ups"], 1, 3, 15),
            PE(UpperBodyProgramId, byName["Dumbbell Press"], 2, 3, 12),
            PE(UpperBodyProgramId, byName["Shoulder Press"], 3, 3, 12),
            PE(UpperBodyProgramId, byName["Bent Over Rows"], 4, 3, 12),
            PE(UpperBodyProgramId, byName["Plank"], 5, 3, 0, 45),
            PE(UpperBodyProgramId, byName["Bicep Curls"], 6, 3, 12),
            PE(UpperBodyProgramId, byName["Tricep Dips"], 7, 3, 10),
            PE(UpperBodyProgramId, byName["Lateral Raises"], 8, 3, 15),
            PE(UpperBodyProgramId, byName["Push Ups"], 9, 2, 20),
            PE(UpperBodyProgramId, byName["Dumbbell Press"], 10, 2, 10),
            PE(UpperBodyProgramId, byName["Plank"], 11, 2, 0, 60),
            PE(UpperBodyProgramId, byName["Shoulder Press"], 12, 2, 10),

            PE(SummerShredProgramId, byName["Battle Ropes"], 1, 4, 30),
            PE(SummerShredProgramId, byName["Burpees"], 2, 3, 15),
            PE(SummerShredProgramId, byName["Jump Squats"], 3, 4, 20),
            PE(SummerShredProgramId, byName["Mountain Climbers"], 4, 3, 30),
            PE(SummerShredProgramId, byName["Plank"], 5, 3, 0, 45),

            PE(FullBodyHiitProgramId, byName["Burpees"], 1, 5, 10),
            PE(FullBodyHiitProgramId, byName["Jump Squats"], 2, 5, 15),
            PE(FullBodyHiitProgramId, byName["Mountain Climbers"], 3, 5, 20),
            PE(FullBodyHiitProgramId, byName["Battle Ropes"], 4, 4, 30)
        ];
    }

    private static ProgramExercise PE(Guid programId, Guid exerciseId, int order, int sets, int reps, int? durationSeconds = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            ProgramId = programId,
            ExerciseId = exerciseId,
            OrderIndex = order,
            Sets = sets,
            Reps = reps,
            DurationSeconds = durationSeconds
        };
}
