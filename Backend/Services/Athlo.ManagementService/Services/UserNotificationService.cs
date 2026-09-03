using Athlo.Models.Helpers;
using Athlo.Repositories.Notifications;
using Athlo.Repositories.Users;
using Athlo.Repositories.Workouts;

namespace Athlo.ManagementService.Services;

public interface IUserNotificationService
{
    Task NotifyWorkoutCompletedAsync(Guid userId, Guid sessionId, CancellationToken ct = default);
    Task SendWorkoutRemindersAsync(CancellationToken ct = default);
}

public class UserNotificationService(
    IUserRepository userRepository,
    IDevicePushTokenRepository tokenRepository,
    IWorkoutSessionRepository sessionRepository,
    IAchievementService achievementService,
    IPushNotificationSender pushSender,
    ILogger<UserNotificationService> logger) : IUserNotificationService
{
    public async Task NotifyWorkoutCompletedAsync(Guid userId, Guid sessionId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user is null)
            return;

        var prefs = UserPreferencesJson.Parse(user.PreferencesJson);
        var tokens = await tokenRepository.GetTokensForUserAsync(userId, ct);
        if (tokens.Count == 0)
            return;

        var newlyUnlocked = await achievementService.EvaluateAndUnlockAsync(userId, ct);
        foreach (var achievement in newlyUnlocked)
        {
            await pushSender.SendAsync(
                tokens,
                "Achievement unlocked!",
                $"{achievement.Title} — {achievement.Subtitle}",
                ct);
        }

        if (prefs.NotifyPrAlerts)
        {
            var records = await sessionRepository.GetPersonalRecordsAsync(userId, ct);
            var fresh = records
                .Where(r => r.AchievedAt >= DateTime.UtcNow.AddMinutes(-10))
                .ToList();
            foreach (var record in fresh.Take(3))
            {
                await pushSender.SendAsync(
                    tokens,
                    "New personal record!",
                    $"{record.ExerciseName}: {record.WeightKg} kg × {record.Reps}",
                    ct);
            }
        }

        if (prefs.NotifyStreakReminders)
        {
            var dates = await sessionRepository.GetCompletedDatesAsync(userId, ct);
            var streak = Athlo.Shared.Helpers.StreakCalculator.Calculate(dates);
            if (streak is 3 or 7 or 14 or 30)
            {
                await pushSender.SendAsync(
                    tokens,
                    "Streak update",
                    $"You're on a {streak}-day streak. Keep it going!",
                    ct);
            }
        }

        logger.LogDebug("Post-workout notifications processed UserId={UserId} SessionId={SessionId}", userId, sessionId);
    }

    public async Task SendWorkoutRemindersAsync(CancellationToken ct = default)
    {
        // Users with tokens who haven't completed a workout in 3+ days and opted into reminders.
        var allTokens = await tokenRepository.GetAllActiveAsync(ct);
        var byUser = allTokens.GroupBy(t => t.UserId);

        foreach (var group in byUser)
        {
            ct.ThrowIfCancellationRequested();
            var user = await userRepository.GetByIdAsync(group.Key, ct);
            if (user is null)
                continue;

            var prefs = UserPreferencesJson.Parse(user.PreferencesJson);
            if (!prefs.NotifyWorkoutReminders)
                continue;

            var dates = await sessionRepository.GetCompletedDatesAsync(group.Key, ct);
            var last = dates.DefaultIfEmpty().Max();
            var daysSince = last == default
                ? 99
                : (DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - last.DayNumber);

            if (daysSince < 3)
                continue;

            await pushSender.SendAsync(
                group.Select(g => g.Token).Distinct().ToList(),
                "Time to train",
                daysSince >= 99
                    ? "Your next workout is waiting. Start a program in Athlo."
                    : $"It's been {daysSince} days since your last workout. Let's get back on track.",
                ct);
        }
    }
}
