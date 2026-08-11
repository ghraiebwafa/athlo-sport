namespace Athlo.Models.DTOs.Progress;

public class PersonalRecordDto
{
    public Guid ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public int Reps { get; set; }
    public DateTime AchievedAt { get; set; }
}
