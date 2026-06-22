using Athlo.Shared.Enums;

namespace Athlo.Models.Entities;

public class WorkoutProgram
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public WorkoutDifficulty Difficulty { get; set; }
    public int EstimatedCalories { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public Guid CategoryId { get; set; }

    public Category Category { get; set; } = null!;
    public ICollection<ProgramExercise> ProgramExercises { get; set; } = [];
    public ICollection<WorkoutSession> WorkoutSessions { get; set; } = [];
}
