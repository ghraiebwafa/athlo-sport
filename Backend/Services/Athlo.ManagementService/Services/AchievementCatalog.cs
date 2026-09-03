using Athlo.Models.DTOs.Achievements;

namespace Athlo.ManagementService.Services;

public static class AchievementCatalog
{
    public static IReadOnlyList<AchievementDefinition> All { get; } =
    [
        new("first", "First Workout", "Completed", "#007AFF", p => p.TotalWorkouts >= 1),
        new("streak7", "7 Day Streak", "Achieved", "#FF9500", p => p.CurrentStreak >= 7),
        new("workouts25", "25 Workouts", "Completed", "#34C759", p => p.TotalWorkouts >= 25),
        new("calories10k", "10K Calories", "Burned", "#AF52DE", p => p.TotalCaloriesBurned >= 10_000)
    ];

    public sealed record AchievementDefinition(
        string Key,
        string Title,
        string Subtitle,
        string Color,
        Func<AchievementProgressSnapshot, bool> IsMet);
}

public sealed record AchievementProgressSnapshot(
    int TotalWorkouts,
    int TotalCaloriesBurned,
    int CurrentStreak);
