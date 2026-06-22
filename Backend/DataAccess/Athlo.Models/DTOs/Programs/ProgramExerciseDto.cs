namespace Athlo.Models.DTOs.Programs;

public class ProgramExerciseDto
{
    public Guid Id { get; set; }
    public Guid ExerciseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public int? DurationSeconds { get; set; }
    public string? ImageUrl { get; set; }
}
