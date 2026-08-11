using Athlo.ManagementService.Services;
using Athlo.Models.DTOs.Workouts;
using Athlo.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.ManagementService.Controllers;

[ApiController]
[Route("api/workouts")]
[Authorize]
[EnableRateLimiting("api")]
public class WorkoutsController(
    IWorkoutService workoutService,
    IValidator<StartWorkoutRequest> startValidator,
    IValidator<CompleteWorkoutRequest> completeValidator,
    IValidator<CancelWorkoutRequest> cancelValidator,
    IValidator<LogSetRequest> logSetValidator,
    IValidator<UpdateSetRequest> updateSetValidator) : ControllerBase
{
    [HttpGet("active")]
    public async Task<ActionResult<WorkoutSessionDto?>> GetActive(CancellationToken ct)
    {
        var active = await workoutService.GetActiveAsync(User.GetUserId(), ct);
        return active is null ? NoContent() : Ok(active);
    }

    [HttpPost("start")]
    public async Task<ActionResult<WorkoutSessionDto>> Start([FromBody] StartWorkoutRequest request, CancellationToken ct)
    {
        var error = await startValidator.ToValidationErrorAsync(request, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await workoutService.StartAsync(User.GetUserId(), request.ProgramId, ct));
    }

    [HttpPost("complete")]
    public async Task<ActionResult<WorkoutSessionDto>> Complete([FromBody] CompleteWorkoutRequest request, CancellationToken ct)
    {
        var error = await completeValidator.ToValidationErrorAsync(request, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await workoutService.CompleteAsync(User.GetUserId(), request.SessionId, request.CaloriesBurned, ct));
    }

    [HttpPost("cancel")]
    public async Task<ActionResult<WorkoutSessionDto>> Cancel([FromBody] CancelWorkoutRequest request, CancellationToken ct)
    {
        var error = await cancelValidator.ToValidationErrorAsync(request, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await workoutService.CancelAsync(User.GetUserId(), request.SessionId, ct));
    }

    [HttpPost("{sessionId:guid}/sets")]
    public async Task<ActionResult<WorkoutSetLogDto>> LogSet(
        Guid sessionId, [FromBody] LogSetRequest request, CancellationToken ct)
    {
        var error = await logSetValidator.ToValidationErrorAsync(request, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await workoutService.LogSetAsync(User.GetUserId(), sessionId, request, ct));
    }

    [HttpPut("sets/{setLogId:guid}")]
    public async Task<ActionResult<WorkoutSetLogDto>> UpdateSet(
        Guid setLogId, [FromBody] UpdateSetRequest request, CancellationToken ct)
    {
        var error = await updateSetValidator.ToValidationErrorAsync(request, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await workoutService.UpdateSetAsync(User.GetUserId(), setLogId, request, ct));
    }

    [HttpGet("history")]
    public async Task<ActionResult<PagedResult<WorkoutSessionDto>>> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        return Ok(await workoutService.GetHistoryAsync(User.GetUserId(), page, pageSize, ct));
    }
}
