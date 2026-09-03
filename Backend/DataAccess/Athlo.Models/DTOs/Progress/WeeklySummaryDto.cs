namespace Athlo.Models.DTOs.Progress;

public class WeeklySummaryDto
{
    public DateOnly WeekStart { get; set; }
    public DateOnly WeekEnd { get; set; }
    public int WorkoutsCompleted { get; set; }
    public int CaloriesBurned { get; set; }
    public int CurrentStreak { get; set; }
    public int MinutesTrained { get; set; }
    public string Headline { get; set; } = string.Empty;
}
