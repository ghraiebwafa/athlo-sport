using Athlo.Shared.Enums;

namespace Athlo.Models.DTOs.Programs;

public class UpdateProgramRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public WorkoutDifficulty Difficulty { get; set; }
    public int EstimatedCalories { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public Guid CategoryId { get; set; }
    public List<ProgramExerciseInput> Exercises { get; set; } = [];
}
