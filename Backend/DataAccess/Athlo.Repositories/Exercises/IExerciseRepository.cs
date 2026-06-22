using Athlo.Models.Entities;

namespace Athlo.Repositories.Exercises;

public interface IExerciseRepository
{
    Task<int> CountAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Exercise>> GetAllAsync(CancellationToken ct = default);
    Task<Exercise?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> IsUsedInProgramsAsync(Guid exerciseId, CancellationToken ct = default);
    Task AddAsync(Exercise exercise, CancellationToken ct = default);
    Task UpdateAsync(Exercise exercise, CancellationToken ct = default);
    Task DeleteAsync(Exercise exercise, CancellationToken ct = default);
}
