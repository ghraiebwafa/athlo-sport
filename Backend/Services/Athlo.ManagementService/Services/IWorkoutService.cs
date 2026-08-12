using Athlo.Models.DTOs.Workouts;

namespace Athlo.ManagementService.Services;

public interface IWorkoutService
{
    Task<WorkoutSessionDto?> GetActiveAsync(Guid userId, CancellationToken ct = default);
    Task<WorkoutSessionDto> StartAsync(Guid userId, Guid programId, CancellationToken ct = default);
    Task<WorkoutSessionDto> CompleteAsync(Guid userId, Guid sessionId, int caloriesBurned, CancellationToken ct = default);
    Task<WorkoutSessionDto> CancelAsync(Guid userId, Guid sessionId, CancellationToken ct = default);
    Task<WorkoutSessionDto> PauseAsync(Guid userId, Guid sessionId, CancellationToken ct = default);
    Task<WorkoutSessionDto> ResumeAsync(Guid userId, Guid sessionId, CancellationToken ct = default);
    Task<WorkoutSetLogDto> LogSetAsync(Guid userId, Guid sessionId, LogSetRequest request, CancellationToken ct = default);
    Task<WorkoutSetLogDto> UpdateSetAsync(Guid userId, Guid setLogId, UpdateSetRequest request, CancellationToken ct = default);
    Task<PagedResult<WorkoutSessionDto>> GetHistoryAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<int> CancelStaleSessionsAsync(TimeSpan maxAge, CancellationToken ct = default);
}
