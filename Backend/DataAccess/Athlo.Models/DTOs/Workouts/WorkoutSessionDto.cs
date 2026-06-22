using Athlo.Shared.Enums;

namespace Athlo.Models.DTOs.Workouts;

public class WorkoutSessionDto
{
    public Guid Id { get; set; }
    public Guid ProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? CaloriesBurned { get; set; }
    public WorkoutSessionStatus Status { get; set; }
    public int? DurationMinutes { get; set; }
}
