using Athlo.Models.DTOs.Progress;
using Athlo.Models.DTOs.Workouts;
using Athlo.Models.Entities;

namespace Athlo.Mapper;

public static class WorkoutMapper
{
    public static WorkoutSessionDto ToDto(WorkoutSession session) =>
        new()
        {
            Id = session.Id,
            ProgramId = session.ProgramId,
            ProgramName = session.Program?.Name ?? string.Empty,
            StartedAt = session.StartedAt,
            CompletedAt = session.CompletedAt,
            CaloriesBurned = session.CaloriesBurned,
            Status = session.Status,
            DurationMinutes = session.CompletedAt.HasValue
                ? (int)Math.Round(ActiveDuration(session, session.CompletedAt.Value).TotalMinutes)
                : null,
            PausedAt = session.PausedAt,
            PausedDurationSeconds = session.PausedDurationSeconds,
            IsPaused = session.PausedAt is not null,
            Sets = session.SetLogs?
                .OrderBy(s => s.LoggedAt)
                .Select(ToSetDto)
                .ToList() ?? []
        };

    public static WorkoutSetLogDto ToSetDto(WorkoutSetLog log) =>
        new()
        {
            Id = log.Id,
            ProgramExerciseId = log.ProgramExerciseId,
            ExerciseId = log.ExerciseId,
            ExerciseName = log.Exercise?.Name ?? string.Empty,
            SetNumber = log.SetNumber,
            RepsCompleted = log.RepsCompleted,
            WeightKg = log.WeightKg,
            Completed = log.Completed,
            LoggedAt = log.LoggedAt
        };

    public static WorkoutHistoryItemDto ToHistoryItem(WorkoutSession session) =>
        new()
        {
            SessionId = session.Id,
            ProgramName = session.Program?.Name ?? string.Empty,
            CompletedAt = session.CompletedAt ?? session.StartedAt,
            CaloriesBurned = session.CaloriesBurned ?? 0,
            DurationMinutes = session.CompletedAt.HasValue
                ? (int)Math.Round(ActiveDuration(session, session.CompletedAt.Value).TotalMinutes)
                : 0
        };

    public static void FinalizeOpenPause(WorkoutSession session, DateTime asOfUtc)
    {
        if (session.PausedAt is null)
            return;

        session.PausedDurationSeconds += (int)Math.Max(0, (asOfUtc - session.PausedAt.Value).TotalSeconds);
        session.PausedAt = null;
    }

    public static TimeSpan ActiveDuration(WorkoutSession session, DateTime asOfUtc)
    {
        var pausedSeconds = session.PausedDurationSeconds;
        if (session.PausedAt is DateTime pausedAt)
            pausedSeconds += (int)Math.Max(0, (asOfUtc - pausedAt).TotalSeconds);

        var wallSeconds = Math.Max(0, (asOfUtc - session.StartedAt).TotalSeconds);
        return TimeSpan.FromSeconds(Math.Max(0, wallSeconds - pausedSeconds));
    }
}
