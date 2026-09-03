using Athlo.Models.DTOs.Progress;

namespace Athlo.ManagementService.Services;

/// <summary>
/// Aggregated fitness progress metrics for an authenticated user.
/// </summary>
public interface IProgressService
{
    /// <summary>
    /// Returns workout statistics and streak data for the authenticated user.
    /// </summary>
    /// <param name="userId">Authenticated user identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Progress summary including completed workouts and current streak.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.NotFoundException">Thrown when the user does not exist.</exception>
    Task<ProgressResponse> GetProgressAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Returns a short weekly training summary for home / retention surfaces.</summary>
    Task<WeeklySummaryDto> GetWeeklySummaryAsync(Guid userId, CancellationToken ct = default);
}
