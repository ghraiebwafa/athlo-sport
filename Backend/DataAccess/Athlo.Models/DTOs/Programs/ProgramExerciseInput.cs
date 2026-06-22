namespace Athlo.Models.DTOs.Programs;

public class ProgramExerciseInput
{
    public Guid ExerciseId { get; set; }
    public int OrderIndex { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public int? DurationSeconds { get; set; }
}
