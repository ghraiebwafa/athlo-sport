using Athlo.Models.Entities;

namespace Athlo.Repositories.Programs;

public interface IProgramRepository
{
    Task<int> CountAsync(CancellationToken ct = default);
    Task<IReadOnlyList<WorkoutProgram>> GetAllAsync(CancellationToken ct = default);
    Task<WorkoutProgram?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WorkoutProgram?> GetTrackedByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> AllExercisesExistAsync(IReadOnlyCollection<Guid> exerciseIds, CancellationToken ct = default);
    Task<bool> HasWorkoutSessionsAsync(Guid programId, CancellationToken ct = default);
    Task AddAsync(WorkoutProgram program, CancellationToken ct = default);
    Task UpdateAsync(WorkoutProgram program, CancellationToken ct = default);
    Task DeleteAsync(WorkoutProgram program, CancellationToken ct = default);
}
