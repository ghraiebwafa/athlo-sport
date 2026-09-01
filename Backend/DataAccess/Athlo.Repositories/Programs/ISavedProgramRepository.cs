namespace Athlo.Repositories.Programs;

public interface ISavedProgramRepository
{
    Task<IReadOnlyList<Guid>> GetSavedProgramIdsAsync(Guid userId, CancellationToken ct = default);
    Task<bool> IsSavedAsync(Guid userId, Guid programId, CancellationToken ct = default);
    Task SaveAsync(Guid userId, Guid programId, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid userId, Guid programId, CancellationToken ct = default);
}
