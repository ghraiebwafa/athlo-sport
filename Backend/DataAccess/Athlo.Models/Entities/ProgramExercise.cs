namespace Athlo.Models.Entities;

public class ProgramExercise
{
    public Guid Id { get; set; }
    public Guid ProgramId { get; set; }
    public Guid ExerciseId { get; set; }
    public int OrderIndex { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public int? DurationSeconds { get; set; }

    public WorkoutProgram Program { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
}
