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

    /// <summary>When set, the session is currently paused.</summary>
    public DateTime? PausedAt { get; set; }

    /// <summary>Total seconds spent paused across completed pause intervals.</summary>
    public int PausedDurationSeconds { get; set; }

    public User User { get; set; } = null!;
    public WorkoutProgram Program { get; set; } = null!;
    public ICollection<WorkoutSetLog> SetLogs { get; set; } = [];
}
