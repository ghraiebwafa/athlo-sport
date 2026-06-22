using Athlo.Database.DbContexts;
using Athlo.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Athlo.Repositories.Exercises;

public class ExerciseRepository(AthloDbContext context) : IExerciseRepository
{
    public Task<int> CountAsync(CancellationToken ct = default) =>
        context.Exercises.CountAsync(ct);

    public async Task<IReadOnlyList<Exercise>> GetAllAsync(CancellationToken ct = default) =>
        await context.Exercises
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .ToListAsync(ct);

    public Task<Exercise?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Exercises.FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<bool> IsUsedInProgramsAsync(Guid exerciseId, CancellationToken ct = default) =>
        context.ProgramExercises.AnyAsync(pe => pe.ExerciseId == exerciseId, ct);

    public async Task AddAsync(Exercise exercise, CancellationToken ct = default) =>
        await context.Exercises.AddAsync(exercise, ct);

    public Task UpdateAsync(Exercise exercise, CancellationToken ct = default)
    {
        context.Exercises.Update(exercise);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Exercise exercise, CancellationToken ct = default)
    {
        context.Exercises.Remove(exercise);
        return Task.CompletedTask;
    }
}
