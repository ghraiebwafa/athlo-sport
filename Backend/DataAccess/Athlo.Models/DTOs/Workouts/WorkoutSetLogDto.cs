namespace Athlo.Models.DTOs.Workouts;

public class WorkoutSetLogDto
{
    public Guid Id { get; set; }
    public Guid ProgramExerciseId { get; set; }
    public Guid ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int SetNumber { get; set; }
    public int RepsCompleted { get; set; }
    public decimal? WeightKg { get; set; }
    public bool Completed { get; set; }
    public DateTime LoggedAt { get; set; }
}
