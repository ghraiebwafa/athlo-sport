using Athlo.Models.DTOs.Progress;
using Athlo.Models.Entities;

namespace Athlo.Repositories.Workouts;

public interface IWorkoutSessionRepository
{
    Task<WorkoutSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WorkoutSession?> GetCompletedSessionAsync(Guid userId, Guid sessionId, CancellationToken ct = default);
    Task<WorkoutSession?> GetActiveSessionAsync(Guid userId, CancellationToken ct = default);
    Task<(IReadOnlyList<WorkoutSession> Items, int TotalCount)> GetHistoryPagedAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<WorkoutSession>> GetCompletedSessionsAsync(Guid userId, CancellationToken ct = default);
    Task<(int TotalCount, int TotalCalories)> GetCompletedAggregatesAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<DateOnly>> GetCompletedDatesAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<(Guid ProgramId, int MaxCalories)>> GetMaxCaloriesPerProgramAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<PersonalRecordDto>> GetPersonalRecordsAsync(Guid userId, CancellationToken ct = default);
    Task<WorkoutSetLog?> GetSetLogAsync(Guid setLogId, CancellationToken ct = default);
    Task<WorkoutSetLog?> FindSetLogAsync(Guid sessionId, Guid programExerciseId, int setNumber, CancellationToken ct = default);
    Task AddSetLogAsync(WorkoutSetLog log, CancellationToken ct = default);
    Task UpdateSetLogAsync(WorkoutSetLog log, CancellationToken ct = default);
    Task<int> CancelStaleSessionsAsync(DateTime startedBefore, CancellationToken ct = default);
    Task<int> CountCompletedTodayAsync(CancellationToken ct = default);
    Task<int> CountActiveAsync(CancellationToken ct = default);
    Task AddAsync(WorkoutSession session, CancellationToken ct = default);
    Task UpdateAsync(WorkoutSession session, CancellationToken ct = default);
}
