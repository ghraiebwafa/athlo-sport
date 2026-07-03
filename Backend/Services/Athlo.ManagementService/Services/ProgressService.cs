using Athlo.Mapper;
using Athlo.Models.DTOs.Progress;
using Athlo.Models.Entities;
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

        var aggregatesTask = sessionRepository.GetCompletedAggregatesAsync(userId, ct);
        var datesTask = sessionRepository.GetCompletedDatesAsync(userId, ct);
        var bestsTask = sessionRepository.GetMaxCaloriesPerProgramAsync(userId, ct);
        var recentTask = sessionRepository.GetHistoryPagedAsync(userId, 1, 10, ct);

        await Task.WhenAll(aggregatesTask, datesTask, bestsTask, recentTask);

        var (totalCount, totalCalories) = aggregatesTask.Result;
        var dates = datesTask.Result;
        var maxCaloriesPerProgram = bestsTask.Result;
        var (recentItems, _) = recentTask.Result;

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
            PersonalBests = maxCaloriesPerProgram.Count,
            GoalProgressPercent = UserMapper.CalculateGoalProgress(
                user.InitialWeight, user.CurrentWeight, user.GoalWeight, user.FitnessGoal),
            CurrentWeight = user.CurrentWeight,
            GoalWeight = user.GoalWeight,
            WeeklyFrequency = weeklyFrequency,
            RecentWorkouts = recentItems.Select(WorkoutMapper.ToHistoryItem).ToList()
        };
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }
}
