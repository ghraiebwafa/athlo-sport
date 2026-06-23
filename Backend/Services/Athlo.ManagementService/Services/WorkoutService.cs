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

        if (session.UserId != userId)
            throw new ForbiddenException("You do not have access to this workout session.");

        if (session.Status != WorkoutSessionStatus.InProgress)
            throw new ConflictException(GetInactiveSessionMessage(session.Status));

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

        if (session.UserId != userId)
            throw new ForbiddenException("You do not have access to this workout session.");

        if (session.Status != WorkoutSessionStatus.InProgress)
            throw new ConflictException(GetInactiveSessionMessage(session.Status));

        session.Status = WorkoutSessionStatus.Cancelled;
        session.CompletedAt = DateTime.UtcNow;

        await sessionRepository.UpdateAsync(session, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return WorkoutMapper.ToDto(session);
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

    private static string GetInactiveSessionMessage(WorkoutSessionStatus status) => status switch
    {
        WorkoutSessionStatus.Completed => "This workout is already completed.",
        WorkoutSessionStatus.Cancelled => "This workout was cancelled.",
        _ => "This workout session is no longer active."
    };
}
