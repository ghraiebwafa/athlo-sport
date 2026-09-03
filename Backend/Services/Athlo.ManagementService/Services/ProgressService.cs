using Athlo.Mapper;
using Athlo.Models.DTOs.Progress;
using Athlo.Repositories.Users;
using Athlo.Repositories.Workouts;
using Athlo.Shared.Exceptions;
using Athlo.Shared.Helpers;

namespace Athlo.ManagementService.Services;

public class ProgressService(
    IUserRepository userRepository,
    IWorkoutSessionRepository sessionRepository) : IProgressService
{
    public async Task<ProgressResponse> GetProgressAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User not found.");

        // Sequential: repositories share one scoped DbContext (EF Core is not thread-safe).
        var (totalCount, totalCalories) = await sessionRepository.GetCompletedAggregatesAsync(userId, ct);
        var dates = await sessionRepository.GetCompletedDatesAsync(userId, ct);
        var personalRecords = await sessionRepository.GetPersonalRecordsAsync(userId, ct);
        var (recentItems, _) = await sessionRepository.GetHistoryPagedAsync(userId, 1, 10, ct);

        var streak = StreakCalculator.Calculate(dates);

        var eightWeeksAgo = DateTime.UtcNow.Date.AddDays(-56);
        var weeklyFrequency = dates
            .Where(d => d.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) >= eightWeeksAgo)
            .GroupBy(d => StartOfWeek(d.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)))
            .OrderBy(g => g.Key)
            .Select(g => new WeeklyWorkoutDto
            {
                WeekStart = DateOnly.FromDateTime(g.Key),
                WorkoutCount = g.Count()
            })
            .ToList();

        return new ProgressResponse
        {
            TotalWorkouts = totalCount,
            TotalCaloriesBurned = totalCalories,
            CurrentStreak = streak,
            PersonalBests = personalRecords.Count,
            GoalProgressPercent = UserMapper.CalculateGoalProgress(
                user.InitialWeight, user.CurrentWeight, user.GoalWeight, user.FitnessGoal),
            CurrentWeight = user.CurrentWeight,
            GoalWeight = user.GoalWeight,
            WeeklyFrequency = weeklyFrequency,
            RecentWorkouts = recentItems.Select(WorkoutMapper.ToHistoryItem).ToList(),
            PersonalRecords = personalRecords
        };
    }

    public async Task<WeeklySummaryDto> GetWeeklySummaryAsync(Guid userId, CancellationToken ct = default)
    {
        _ = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User not found.");

        var weekStart = DateOnly.FromDateTime(StartOfWeek(DateTime.UtcNow));
        var weekEnd = weekStart.AddDays(6);
        var (sessions, _) = await sessionRepository.GetHistoryPagedAsync(userId, 1, 50, ct);
        var thisWeek = sessions
            .Where(s => s.CompletedAt is not null
                        && DateOnly.FromDateTime(s.CompletedAt.Value) >= weekStart
                        && DateOnly.FromDateTime(s.CompletedAt.Value) <= weekEnd)
            .ToList();

        var dates = await sessionRepository.GetCompletedDatesAsync(userId, ct);
        var streak = StreakCalculator.Calculate(dates);
        var workouts = thisWeek.Count;
        var calories = thisWeek.Sum(s => s.CaloriesBurned ?? 0);
        var minutes = thisWeek.Sum(s =>
            s.CompletedAt.HasValue
                ? (int)Math.Round(WorkoutMapper.ActiveDuration(s, s.CompletedAt.Value).TotalMinutes)
                : 0);

        var headline = workouts switch
        {
            0 => "No workouts yet this week — a short session still counts.",
            1 => "Nice start — one workout logged this week.",
            >= 4 => "Strong week! You're building real consistency.",
            _ => $"Solid progress — {workouts} workouts this week."
        };

        return new WeeklySummaryDto
        {
            WeekStart = weekStart,
            WeekEnd = weekEnd,
            WorkoutsCompleted = workouts,
            CaloriesBurned = calories,
            CurrentStreak = streak,
            MinutesTrained = minutes,
            Headline = headline
        };
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }
}
