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

        var sessions = await sessionRepository.GetCompletedSessionsAsync(userId, ct);
        var totalCalories = sessions.Sum(s => s.CaloriesBurned ?? 0);

        var workoutDates = sessions
            .Where(s => s.CompletedAt.HasValue)
            .Select(s => DateOnly.FromDateTime(s.CompletedAt!.Value));
        var streak = StreakCalculator.Calculate(workoutDates);

        var personalBests = CountPersonalBests(sessions);

        var weeklyFrequency = sessions
            .Where(s => s.CompletedAt.HasValue)
            .GroupBy(s => StartOfWeek(s.CompletedAt!.Value))
            .OrderByDescending(g => g.Key)
            .Take(8)
            .OrderBy(g => g.Key)
            .Select(g => new WeeklyWorkoutDto
            {
                WeekStart = DateOnly.FromDateTime(g.Key),
                WorkoutCount = g.Count()
            })
            .ToList();

        var (recentItems, _) = await sessionRepository.GetHistoryPagedAsync(userId, 1, 10, ct);

        return new ProgressResponse
        {
            TotalWorkouts = sessions.Count,
            TotalCaloriesBurned = totalCalories,
            CurrentStreak = streak,
            PersonalBests = personalBests,
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

    private static int CountPersonalBests(IReadOnlyList<WorkoutSession> sessions)
    {
        var personalBests = 0;
        var bestCaloriesByProgram = new Dictionary<Guid, int>();

        foreach (var session in sessions.Where(s => s.CompletedAt.HasValue).OrderBy(s => s.CompletedAt))
        {
            var calories = session.CaloriesBurned ?? 0;

            if (bestCaloriesByProgram.TryGetValue(session.ProgramId, out var currentBest))
            {
                if (calories > currentBest)
                {
                    personalBests++;
                    bestCaloriesByProgram[session.ProgramId] = calories;
                }
            }
            else
            {
                personalBests++;
                bestCaloriesByProgram[session.ProgramId] = calories;
            }
        }

        return personalBests;
    }
}
