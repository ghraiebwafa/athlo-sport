using Athlo.Database.DbContexts;
using Athlo.Models.DTOs.Progress;
using Athlo.Models.Entities;
using Athlo.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Athlo.Repositories.Workouts;

public class WorkoutSessionRepository(AthloDbContext context) : IWorkoutSessionRepository
{
    public Task<WorkoutSession?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.WorkoutSessions
            .Include(s => s.Program)
            .Include(s => s.SetLogs)
                .ThenInclude(l => l.Exercise)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<WorkoutSession?> GetCompletedSessionAsync(Guid userId, Guid sessionId, CancellationToken ct = default) =>
        context.WorkoutSessions
            .AsNoTracking()
            .Include(s => s.Program)
            .Include(s => s.SetLogs)
                .ThenInclude(l => l.Exercise)
            .FirstOrDefaultAsync(
                s => s.Id == sessionId
                     && s.UserId == userId
                     && s.Status == WorkoutSessionStatus.Completed,
                ct);

    public Task<WorkoutSession?> GetActiveSessionAsync(Guid userId, CancellationToken ct = default) =>
        context.WorkoutSessions
            .Include(s => s.Program)
            .Include(s => s.SetLogs)
                .ThenInclude(l => l.Exercise)
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

    public async Task<IReadOnlyList<WorkoutSession>> GetSessionsForExportAsync(Guid userId, CancellationToken ct = default) =>
        await context.WorkoutSessions
            .AsNoTracking()
            .Include(s => s.Program)
            .Include(s => s.SetLogs)
                .ThenInclude(l => l.Exercise)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(ct);

    public async Task<(int TotalCount, int TotalCalories)> GetCompletedAggregatesAsync(Guid userId, CancellationToken ct = default)
    {
        var result = await context.WorkoutSessions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.Status == WorkoutSessionStatus.Completed)
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Calories = g.Sum(s => s.CaloriesBurned ?? 0) })
            .FirstOrDefaultAsync(ct);

        return result is null ? (0, 0) : (result.Count, result.Calories);
    }

    public async Task<IReadOnlyList<DateOnly>> GetCompletedDatesAsync(Guid userId, CancellationToken ct = default) =>
        await context.WorkoutSessions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.Status == WorkoutSessionStatus.Completed && s.CompletedAt != null)
            .Select(s => DateOnly.FromDateTime(s.CompletedAt!.Value))
            .Distinct()
            .ToListAsync(ct);

    public async Task<IReadOnlyList<(Guid ProgramId, int MaxCalories)>> GetMaxCaloriesPerProgramAsync(Guid userId, CancellationToken ct = default)
    {
        var results = await context.WorkoutSessions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.Status == WorkoutSessionStatus.Completed && s.CaloriesBurned != null)
            .GroupBy(s => s.ProgramId)
            .Select(g => new { ProgramId = g.Key, MaxCalories = g.Max(s => s.CaloriesBurned!.Value) })
            .ToListAsync(ct);

        return results.Select(r => (r.ProgramId, r.MaxCalories)).ToList();
    }

    public async Task<IReadOnlyList<PersonalRecordDto>> GetPersonalRecordsAsync(Guid userId, CancellationToken ct = default)
    {
        var logs = await context.WorkoutSetLogs
            .AsNoTracking()
            .Include(l => l.Exercise)
            .Include(l => l.Session)
            .Where(l =>
                l.Session.UserId == userId
                && l.Completed
                && l.WeightKg != null
                && l.WeightKg > 0
                && (l.Session.Status == WorkoutSessionStatus.Completed
                    || l.Session.Status == WorkoutSessionStatus.InProgress))
            .ToListAsync(ct);

        return logs
            .GroupBy(l => l.ExerciseId)
            .Select(g =>
            {
                var best = g
                    .OrderByDescending(l => l.WeightKg)
                    .ThenByDescending(l => l.RepsCompleted)
                    .ThenByDescending(l => l.LoggedAt)
                    .First();
                return new PersonalRecordDto
                {
                    ExerciseId = best.ExerciseId,
                    ExerciseName = best.Exercise?.Name ?? string.Empty,
                    WeightKg = best.WeightKg!.Value,
                    Reps = best.RepsCompleted,
                    AchievedAt = best.LoggedAt
                };
            })
            .OrderByDescending(r => r.WeightKg)
            .ThenBy(r => r.ExerciseName)
            .ToList();
    }

    public Task<WorkoutSetLog?> GetSetLogAsync(Guid setLogId, CancellationToken ct = default) =>
        context.WorkoutSetLogs
            .Include(l => l.Exercise)
            .Include(l => l.Session)
            .FirstOrDefaultAsync(l => l.Id == setLogId, ct);

    public Task<WorkoutSetLog?> FindSetLogAsync(
        Guid sessionId, Guid programExerciseId, int setNumber, CancellationToken ct = default) =>
        context.WorkoutSetLogs
            .Include(l => l.Exercise)
            .FirstOrDefaultAsync(
                l => l.SessionId == sessionId
                     && l.ProgramExerciseId == programExerciseId
                     && l.SetNumber == setNumber,
                ct);

    public async Task AddSetLogAsync(WorkoutSetLog log, CancellationToken ct = default) =>
        await context.WorkoutSetLogs.AddAsync(log, ct);

    public Task UpdateSetLogAsync(WorkoutSetLog log, CancellationToken ct = default)
    {
        context.WorkoutSetLogs.Update(log);
        return Task.CompletedTask;
    }

    public async Task<int> CancelStaleSessionsAsync(DateTime startedBefore, CancellationToken ct = default)
    {
        var stale = await context.WorkoutSessions
            .Where(s => s.Status == WorkoutSessionStatus.InProgress && s.StartedAt < startedBefore)
            .ToListAsync(ct);

        if (stale.Count == 0)
            return 0;

        var completedAt = DateTime.UtcNow;
        foreach (var session in stale)
        {
            session.Status = WorkoutSessionStatus.Cancelled;
            session.CompletedAt = completedAt;
        }

        await context.SaveChangesAsync(ct);
        return stale.Count;
    }

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
