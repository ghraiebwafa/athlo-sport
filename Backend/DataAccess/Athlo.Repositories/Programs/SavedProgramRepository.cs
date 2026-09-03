using Athlo.Database.DbContexts;
using Athlo.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Athlo.Repositories.Programs;

public class SavedProgramRepository(AthloDbContext context) : ISavedProgramRepository
{
    public async Task<IReadOnlyList<Guid>> GetSavedProgramIdsAsync(Guid userId, CancellationToken ct = default) =>
        await context.SavedPrograms
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.SavedAt)
            .Select(s => s.ProgramId)
            .ToListAsync(ct);

    public Task<bool> IsSavedAsync(Guid userId, Guid programId, CancellationToken ct = default) =>
        context.SavedPrograms.AnyAsync(s => s.UserId == userId && s.ProgramId == programId, ct);

    public async Task SaveAsync(Guid userId, Guid programId, CancellationToken ct = default)
    {
        if (await IsSavedAsync(userId, programId, ct))
            return;

        await context.SavedPrograms.AddAsync(new SavedProgram
        {
            UserId = userId,
            ProgramId = programId,
            SavedAt = DateTime.UtcNow
        }, ct);
    }

    public async Task<bool> RemoveAsync(Guid userId, Guid programId, CancellationToken ct = default)
    {
        var row = await context.SavedPrograms
            .FirstOrDefaultAsync(s => s.UserId == userId && s.ProgramId == programId, ct);
        if (row is null)
            return false;

        context.SavedPrograms.Remove(row);
        return true;
    }

    public async Task<IReadOnlyList<SavedProgram>> GetSavedWithProgramsAsync(Guid userId, CancellationToken ct = default) =>
        await context.SavedPrograms
            .AsNoTracking()
            .Include(s => s.Program)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.SavedAt)
            .ToListAsync(ct);
}
