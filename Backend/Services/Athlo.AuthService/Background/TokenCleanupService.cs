using Athlo.Repositories.PasswordResetTokens;
using Athlo.Repositories.RefreshTokens;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Athlo.AuthService.Background;

/// <summary>Periodically removes expired and long-revoked auth tokens.</summary>
public class TokenCleanupService(IServiceScopeFactory scopeFactory, ILogger<TokenCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);
    private static readonly TimeSpan RefreshRetention = TimeSpan.FromDays(30);
    private static readonly TimeSpan ResetRetention = TimeSpan.FromDays(7);

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
                logger.LogError(ex, "Token cleanup failed");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var refreshRepo = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
        var resetRepo = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenRepository>();

        var refreshCutoff = DateTime.UtcNow.Subtract(RefreshRetention);
        var resetCutoff = DateTime.UtcNow.Subtract(ResetRetention);

        var refreshDeleted = await refreshRepo.DeleteExpiredAsync(refreshCutoff, ct);
        var resetDeleted = await resetRepo.DeleteExpiredAsync(resetCutoff, ct);

        if (refreshDeleted > 0 || resetDeleted > 0)
        {
            logger.LogInformation(
                "Token cleanup removed {RefreshCount} refresh tokens and {ResetCount} password reset tokens",
                refreshDeleted,
                resetDeleted);
        }
    }
}

public static class TokenCleanupExtensions
{
    public static IServiceCollection AddAthloTokenCleanup(this IServiceCollection services) =>
        services.AddHostedService<TokenCleanupService>();
}
