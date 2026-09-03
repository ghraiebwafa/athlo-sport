using Athlo.ManagementService.Services;

namespace Athlo.ManagementService.Background;

/// <summary>Daily workout reminder push for inactive users who opted in.</summary>
public class WorkoutReminderService(
    IServiceScopeFactory scopeFactory,
    ILogger<WorkoutReminderService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Stagger first run so API can finish startup.
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var notifications = scope.ServiceProvider.GetRequiredService<IUserNotificationService>();
                await notifications.SendWorkoutRemindersAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Workout reminder job failed");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}

public static class WorkoutReminderExtensions
{
    public static IServiceCollection AddAthloWorkoutReminders(this IServiceCollection services) =>
        services.AddHostedService<WorkoutReminderService>();
}
