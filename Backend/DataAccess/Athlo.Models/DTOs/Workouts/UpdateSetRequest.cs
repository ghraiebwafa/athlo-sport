namespace Athlo.Models.DTOs.Workouts;

public class UpdateSetRequest
{
    public int RepsCompleted { get; set; }
    public decimal? WeightKg { get; set; }
    public bool Completed { get; set; } = true;
}
