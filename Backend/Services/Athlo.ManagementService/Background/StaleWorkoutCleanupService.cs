using Athlo.ManagementService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Athlo.ManagementService.Background;

/// <summary>Cancels abandoned InProgress workout sessions so users can start new ones.</summary>
public class StaleWorkoutCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<StaleWorkoutCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);
    private static readonly TimeSpan MaxAge = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Stale workout cleanup failed");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var workoutService = scope.ServiceProvider.GetRequiredService<IWorkoutService>();
        var cancelled = await workoutService.CancelStaleSessionsAsync(MaxAge, ct);
        if (cancelled > 0)
        {
            logger.LogInformation("Stale workout cleanup cancelled {Count} sessions older than {MaxAge}", cancelled, MaxAge);
        }
    }
}

public static class StaleWorkoutCleanupExtensions
{
    public static IServiceCollection AddAthloStaleWorkoutCleanup(this IServiceCollection services) =>
        services.AddHostedService<StaleWorkoutCleanupService>();
}
