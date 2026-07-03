using Athlo.Models.Entities;

namespace Athlo.Repositories.Workouts;

public interface IWorkoutSessionRepository
{
    Task<WorkoutSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WorkoutSession?> GetActiveSessionAsync(Guid userId, CancellationToken ct = default);
    Task<(IReadOnlyList<WorkoutSession> Items, int TotalCount)> GetHistoryPagedAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<WorkoutSession>> GetCompletedSessionsAsync(Guid userId, CancellationToken ct = default);
    Task<(int TotalCount, int TotalCalories)> GetCompletedAggregatesAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<DateOnly>> GetCompletedDatesAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<(Guid ProgramId, int MaxCalories)>> GetMaxCaloriesPerProgramAsync(Guid userId, CancellationToken ct = default);
    Task<int> CountCompletedTodayAsync(CancellationToken ct = default);
    Task<int> CountActiveAsync(CancellationToken ct = default);
    Task AddAsync(WorkoutSession session, CancellationToken ct = default);
    Task UpdateAsync(WorkoutSession session, CancellationToken ct = default);
}
