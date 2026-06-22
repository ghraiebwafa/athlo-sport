using Athlo.Shared.Enums;

namespace Athlo.Models.Entities;

public class WorkoutSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProgramId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? CaloriesBurned { get; set; }
    public WorkoutSessionStatus Status { get; set; } = WorkoutSessionStatus.InProgress;

    public User User { get; set; } = null!;
    public WorkoutProgram Program { get; set; } = null!;
}
