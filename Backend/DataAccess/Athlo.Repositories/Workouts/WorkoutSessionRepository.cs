using Athlo.Database.DbContexts;
using Athlo.Models.Entities;
using Athlo.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Athlo.Repositories.Workouts;

public class WorkoutSessionRepository(AthloDbContext context) : IWorkoutSessionRepository
{
    public Task<WorkoutSession?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.WorkoutSessions
            .Include(s => s.Program)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<WorkoutSession?> GetActiveSessionAsync(Guid userId, CancellationToken ct = default) =>
        context.WorkoutSessions
            .Include(s => s.Program)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == WorkoutSessionStatus.InProgress, ct);

    public async Task<(IReadOnlyList<WorkoutSession> Items, int TotalCount)> GetHistoryPagedAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = context.WorkoutSessions
            .AsNoTracking()
            .Include(s => s.Program)
            .Where(s => s.UserId == userId && s.Status == WorkoutSessionStatus.Completed);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(s => s.CompletedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<WorkoutSession>> GetCompletedSessionsAsync(Guid userId, CancellationToken ct = default) =>
        await context.WorkoutSessions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.Status == WorkoutSessionStatus.Completed && s.CompletedAt != null)
            .OrderByDescending(s => s.CompletedAt)
            .ToListAsync(ct);

    public Task<int> CountCompletedTodayAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        return context.WorkoutSessions.CountAsync(
            s => s.Status == WorkoutSessionStatus.Completed
                 && s.CompletedAt >= today
                 && s.CompletedAt < tomorrow,
            ct);
    }

    public Task<int> CountActiveAsync(CancellationToken ct = default) =>
        context.WorkoutSessions.CountAsync(s => s.Status == WorkoutSessionStatus.InProgress, ct);

    public async Task AddAsync(WorkoutSession session, CancellationToken ct = default) =>
        await context.WorkoutSessions.AddAsync(session, ct);

    public Task UpdateAsync(WorkoutSession session, CancellationToken ct = default)
    {
        context.WorkoutSessions.Update(session);
        return Task.CompletedTask;
    }
}
