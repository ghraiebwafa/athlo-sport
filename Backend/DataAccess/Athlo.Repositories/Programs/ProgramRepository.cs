using Athlo.Database.DbContexts;
using Athlo.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Athlo.Repositories.Programs;

public class ProgramRepository(AthloDbContext context) : IProgramRepository
{
    public Task<int> CountAsync(CancellationToken ct = default) =>
        context.WorkoutPrograms.CountAsync(ct);

    public async Task<IReadOnlyList<WorkoutProgram>> GetAllAsync(CancellationToken ct = default) =>
        await context.WorkoutPrograms
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.ProgramExercises)
            .OrderByDescending(p => p.IsFeatured)
            .ThenBy(p => p.Name)
            .ToListAsync(ct);

    public Task<WorkoutProgram?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.WorkoutPrograms
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.ProgramExercises)
                .ThenInclude(pe => pe.Exercise)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<WorkoutProgram?> GetTrackedByIdAsync(Guid id, CancellationToken ct = default) =>
        context.WorkoutPrograms
            .Include(p => p.ProgramExercises)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<bool> AllExercisesExistAsync(IReadOnlyCollection<Guid> exerciseIds, CancellationToken ct = default)
    {
        if (exerciseIds.Count == 0)
            return false;

        var distinctIds = exerciseIds.Distinct().ToList();
        var found = await context.Exercises.CountAsync(e => distinctIds.Contains(e.Id), ct);
        return found == distinctIds.Count;
    }

    public Task<bool> HasWorkoutSessionsAsync(Guid programId, CancellationToken ct = default) =>
        context.WorkoutSessions.AnyAsync(s => s.ProgramId == programId, ct);

    public async Task AddAsync(WorkoutProgram program, CancellationToken ct = default) =>
        await context.WorkoutPrograms.AddAsync(program, ct);

    public async Task ReplaceExercisesAsync(
        Guid programId,
        IReadOnlyList<ProgramExercise> exercises,
        CancellationToken ct = default)
    {
        var existing = await context.ProgramExercises
            .Where(pe => pe.ProgramId == programId)
            .ToListAsync(ct);

        if (existing.Count > 0)
        {
            context.ProgramExercises.RemoveRange(existing);

            // Keep any tracked parent navigation in sync so EF does not try to
            // update orphaned children on the next SaveChanges.
            var trackedParent = context.ChangeTracker
                .Entries<WorkoutProgram>()
                .Select(e => e.Entity)
                .FirstOrDefault(p => p.Id == programId);
            trackedParent?.ProgramExercises.Clear();

            // Flush deletes before inserts so unique (ProgramId, OrderIndex) stays valid.
            await context.SaveChangesAsync(ct);
        }

        await context.ProgramExercises.AddRangeAsync(exercises, ct);
    }

    public Task DeleteAsync(WorkoutProgram program, CancellationToken ct = default)
    {
        context.WorkoutPrograms.Remove(program);
        return Task.CompletedTask;
    }
}
