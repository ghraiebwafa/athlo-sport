using Athlo.Models.DTOs.Programs;
using Athlo.Models.Entities;

namespace Athlo.Mapper;

public static class ProgramMapper
{
    public static ProgramListItemDto ToListItem(WorkoutProgram program) =>
        new()
        {
            Id = program.Id,
            Name = program.Name,
            Description = program.Description,
            DurationMinutes = program.DurationMinutes,
            Difficulty = program.Difficulty,
            EstimatedCalories = program.EstimatedCalories,
            ImageUrl = program.ImageUrl,
            IsFeatured = program.IsFeatured,
            CategoryName = program.Category?.Name ?? string.Empty,
            ExerciseCount = program.ProgramExercises.Count
        };

    public static ProgramDetailDto ToDetail(WorkoutProgram program) =>
        new()
        {
            Id = program.Id,
            Name = program.Name,
            Description = program.Description,
            DurationMinutes = program.DurationMinutes,
            Difficulty = program.Difficulty,
            EstimatedCalories = program.EstimatedCalories,
            ImageUrl = program.ImageUrl,
            IsFeatured = program.IsFeatured,
            CategoryId = program.CategoryId,
            CategoryName = program.Category?.Name ?? string.Empty,
            Exercises = program.ProgramExercises
                .OrderBy(pe => pe.OrderIndex)
                .Select(pe => new ProgramExerciseDto
                {
                    Id = pe.Id,
                    ExerciseId = pe.ExerciseId,
                    Name = pe.Exercise.Name,
                    OrderIndex = pe.OrderIndex,
                    Sets = pe.Sets,
                    Reps = pe.Reps,
                    DurationSeconds = pe.DurationSeconds,
                    ImageUrl = pe.Exercise.ImageUrl
                })
                .ToList()
        };

    public static CategoryDto ToCategory(Category category) =>
        new()
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Icon = category.Icon,
            ProgramCount = category.Programs.Count
        };
}
