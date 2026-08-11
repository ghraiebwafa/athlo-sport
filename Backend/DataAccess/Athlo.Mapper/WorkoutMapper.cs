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
                ? (int)Math.Round((session.CompletedAt.Value - session.StartedAt).TotalMinutes)
                : null,
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
                ? (int)Math.Round((session.CompletedAt.Value - session.StartedAt).TotalMinutes)
                : 0
        };
}
