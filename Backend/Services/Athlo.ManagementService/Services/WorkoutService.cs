using Athlo.Mapper;
using Athlo.Models.DTOs.Workouts;
using Athlo.Models.Entities;
using Athlo.Repositories;
using Athlo.Repositories.Programs;
using Athlo.Repositories.Workouts;
using Athlo.Shared.Enums;
using Athlo.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Athlo.ManagementService.Services;

public class WorkoutService(
    IWorkoutSessionRepository sessionRepository,
    IProgramRepository programRepository,
    IUnitOfWork unitOfWork) : IWorkoutService
{
    private const int MaxCaloriesMultiplier = 2;
    private const int MaxSetNumber = 50;
    private const int MaxReps = 500;
    private const decimal MaxWeightKg = 1000m;

    public async Task<WorkoutSessionDto?> GetActiveAsync(Guid userId, CancellationToken ct = default)
    {
        var active = await sessionRepository.GetActiveSessionAsync(userId, ct);
        return active is null ? null : WorkoutMapper.ToDto(active);
    }

    public async Task<WorkoutSessionDto> StartAsync(Guid userId, Guid programId, CancellationToken ct = default)
    {
        _ = await programRepository.GetByIdAsync(programId, ct)
            ?? throw new NotFoundException("Workout program not found.");

        var active = await sessionRepository.GetActiveSessionAsync(userId, ct);
        if (active is not null)
            throw new ConflictException("You already have an active workout session.");

        var session = new WorkoutSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProgramId = programId,
            StartedAt = DateTime.UtcNow,
            Status = WorkoutSessionStatus.InProgress
        };

        await sessionRepository.AddAsync(session, ct);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("You already have an active workout session.");
        }

        var created = await sessionRepository.GetByIdAsync(session.Id, ct);
        return WorkoutMapper.ToDto(created!);
    }

    public async Task<WorkoutSessionDto> CompleteAsync(Guid userId, Guid sessionId, int caloriesBurned, CancellationToken ct = default)
    {
        var session = await sessionRepository.GetByIdAsync(sessionId, ct)
            ?? throw new NotFoundException("Workout session not found.");

        EnsureOwnedInProgress(session, userId);

        var program = session.Program ?? await programRepository.GetByIdAsync(session.ProgramId, ct);
        var maxCalories = (program?.EstimatedCalories ?? 500) * MaxCaloriesMultiplier;
        if (caloriesBurned < 0 || caloriesBurned > maxCalories)
            throw new AppException($"Calories burned must be between 0 and {maxCalories}.", 400);

        session.CompletedAt = DateTime.UtcNow;
        session.CaloriesBurned = caloriesBurned;
        session.Status = WorkoutSessionStatus.Completed;

        await sessionRepository.UpdateAsync(session, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return WorkoutMapper.ToDto(session);
    }

    public async Task<WorkoutSessionDto> CancelAsync(Guid userId, Guid sessionId, CancellationToken ct = default)
    {
        var session = await sessionRepository.GetByIdAsync(sessionId, ct)
            ?? throw new NotFoundException("Workout session not found.");

        EnsureOwnedInProgress(session, userId);

        session.Status = WorkoutSessionStatus.Cancelled;
        session.CompletedAt = DateTime.UtcNow;

        await sessionRepository.UpdateAsync(session, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return WorkoutMapper.ToDto(session);
    }

    public async Task<WorkoutSetLogDto> LogSetAsync(
        Guid userId, Guid sessionId, LogSetRequest request, CancellationToken ct = default)
    {
        var session = await sessionRepository.GetByIdAsync(sessionId, ct)
            ?? throw new NotFoundException("Workout session not found.");

        EnsureOwnedInProgress(session, userId);
        ValidateSetValues(request.SetNumber, request.RepsCompleted, request.WeightKg);

        var program = await programRepository.GetByIdAsync(session.ProgramId, ct)
            ?? throw new NotFoundException("Workout program not found.");

        var programExercise = program.ProgramExercises.FirstOrDefault(pe => pe.Id == request.ProgramExerciseId)
            ?? throw new NotFoundException("Exercise is not part of this workout program.");

        var existing = await sessionRepository.FindSetLogAsync(
            sessionId, request.ProgramExerciseId, request.SetNumber, ct);

        if (existing is not null)
        {
            existing.RepsCompleted = request.RepsCompleted;
            existing.WeightKg = request.WeightKg;
            existing.Completed = request.Completed;
            existing.LoggedAt = DateTime.UtcNow;
            await sessionRepository.UpdateSetLogAsync(existing, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return WorkoutMapper.ToSetDto(existing);
        }

        var log = new WorkoutSetLog
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            ProgramExerciseId = programExercise.Id,
            ExerciseId = programExercise.ExerciseId,
            SetNumber = request.SetNumber,
            RepsCompleted = request.RepsCompleted,
            WeightKg = request.WeightKg,
            Completed = request.Completed,
            LoggedAt = DateTime.UtcNow
        };

        await sessionRepository.AddSetLogAsync(log, ct);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("This set was already logged.");
        }

        var saved = await sessionRepository.GetSetLogAsync(log.Id, ct);
        return WorkoutMapper.ToSetDto(saved!);
    }

    public async Task<WorkoutSetLogDto> UpdateSetAsync(
        Guid userId, Guid setLogId, UpdateSetRequest request, CancellationToken ct = default)
    {
        var log = await sessionRepository.GetSetLogAsync(setLogId, ct)
            ?? throw new NotFoundException("Set log not found.");

        if (log.Session.UserId != userId)
            throw new ForbiddenException("You do not have access to this workout session.");

        if (log.Session.Status != WorkoutSessionStatus.InProgress)
            throw new ConflictException(GetInactiveSessionMessage(log.Session.Status));

        ValidateSetValues(log.SetNumber, request.RepsCompleted, request.WeightKg);

        log.RepsCompleted = request.RepsCompleted;
        log.WeightKg = request.WeightKg;
        log.Completed = request.Completed;
        log.LoggedAt = DateTime.UtcNow;

        await sessionRepository.UpdateSetLogAsync(log, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return WorkoutMapper.ToSetDto(log);
    }

    public async Task<PagedResult<WorkoutSessionDto>> GetHistoryAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, totalCount) = await sessionRepository.GetHistoryPagedAsync(userId, page, pageSize, ct);

        return new PagedResult<WorkoutSessionDto>
        {
            Items = items.Select(WorkoutMapper.ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public Task<int> CancelStaleSessionsAsync(TimeSpan maxAge, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.Subtract(maxAge);
        return sessionRepository.CancelStaleSessionsAsync(cutoff, ct);
    }

    private static void EnsureOwnedInProgress(WorkoutSession session, Guid userId)
    {
        if (session.UserId != userId)
            throw new ForbiddenException("You do not have access to this workout session.");

        if (session.Status != WorkoutSessionStatus.InProgress)
            throw new ConflictException(GetInactiveSessionMessage(session.Status));
    }

    private static void ValidateSetValues(int setNumber, int repsCompleted, decimal? weightKg)
    {
        if (setNumber is < 1 or > MaxSetNumber)
            throw new AppException($"Set number must be between 1 and {MaxSetNumber}.", 400);
        if (repsCompleted is < 0 or > MaxReps)
            throw new AppException($"Reps must be between 0 and {MaxReps}.", 400);
        if (weightKg is < 0 or > MaxWeightKg)
            throw new AppException($"Weight must be between 0 and {MaxWeightKg} kg.", 400);
    }

    private static string GetInactiveSessionMessage(WorkoutSessionStatus status) => status switch
    {
        WorkoutSessionStatus.Completed => "This workout is already completed.",
        WorkoutSessionStatus.Cancelled => "This workout was cancelled.",
        _ => "This workout session is no longer active."
    };
}
