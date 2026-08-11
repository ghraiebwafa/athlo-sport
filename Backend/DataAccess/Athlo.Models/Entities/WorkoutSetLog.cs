namespace Athlo.Models.Entities;

public class WorkoutSetLog
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid ProgramExerciseId { get; set; }
    public Guid ExerciseId { get; set; }
    public int SetNumber { get; set; }
    public int RepsCompleted { get; set; }
    public decimal? WeightKg { get; set; }
    public bool Completed { get; set; } = true;
    public DateTime LoggedAt { get; set; }

    public WorkoutSession Session { get; set; } = null!;
    public ProgramExercise ProgramExercise { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
}
