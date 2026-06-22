namespace Athlo.Models.DTOs.Progress;

public class ProgressResponse
{
    public int TotalWorkouts { get; set; }
    public int TotalCaloriesBurned { get; set; }
    public int CurrentStreak { get; set; }
    public int PersonalBests { get; set; }
    public double GoalProgressPercent { get; set; }
    public decimal CurrentWeight { get; set; }
    public decimal GoalWeight { get; set; }
    public IReadOnlyList<WeeklyWorkoutDto> WeeklyFrequency { get; set; } = [];
    public IReadOnlyList<WorkoutHistoryItemDto> RecentWorkouts { get; set; } = [];
}

public class WeeklyWorkoutDto
{
    public DateOnly WeekStart { get; set; }
    public int WorkoutCount { get; set; }
}

public class WorkoutHistoryItemDto
{
    public Guid SessionId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public int CaloriesBurned { get; set; }
    public int DurationMinutes { get; set; }
}
