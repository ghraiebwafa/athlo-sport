using Athlo.ManagementService.Services;
using Athlo.Models.DTOs.Workouts;
using Athlo.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.ManagementService.Controllers;

/// <summary>
/// Manages workout sessions including starting, pausing, logging sets, and viewing history.
/// </summary>
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
    /// <summary>
    /// Returns the authenticated user's currently active workout session, if any.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The active session, or no content if none is in progress.</returns>
    /// <response code="200">Active session found.</response>
    /// <response code="204">No active session.</response>
    /// <response code="401">Authentication required.</response>
    /// <remarks>Returns 204 No Content when the user has no active workout.</remarks>
    [HttpGet("active")]
    public async Task<ActionResult<WorkoutSessionDto?>> GetActive(CancellationToken ct)
    {
        var active = await workoutService.GetActiveAsync(User.GetUserId(), ct);
        return active is null ? NoContent() : Ok(active);
    }

    /// <summary>
    /// Starts a new workout session for the given program.
    /// </summary>
    /// <param name="request">The program to start a workout from.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created workout session.</returns>
    /// <response code="200">Workout started successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Workout program not found.</response>
    [HttpPost("start")]
    public async Task<ActionResult<WorkoutSessionDto>> Start([FromBody] StartWorkoutRequest request, CancellationToken ct)
    {
        var error = await startValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await workoutService.StartAsync(User.GetUserId(), request.ProgramId, ct));
    }

    /// <summary>
    /// Marks an active workout session as completed.
    /// </summary>
    /// <param name="request">Session ID and optional calories burned.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The completed workout session.</returns>
    /// <response code="200">Workout completed successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Workout session not found.</response>
    [HttpPost("complete")]
    public async Task<ActionResult<WorkoutSessionDto>> Complete([FromBody] CompleteWorkoutRequest request, CancellationToken ct)
    {
        var error = await completeValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await workoutService.CompleteAsync(User.GetUserId(), request.SessionId, request.CaloriesBurned, ct));
    }

    /// <summary>
    /// Cancels an active workout session.
    /// </summary>
    /// <param name="request">The session to cancel.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The cancelled workout session.</returns>
    /// <response code="200">Workout cancelled successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Workout session not found.</response>
    [HttpPost("cancel")]
    public async Task<ActionResult<WorkoutSessionDto>> Cancel([FromBody] CancelWorkoutRequest request, CancellationToken ct)
    {
        var error = await cancelValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await workoutService.CancelAsync(User.GetUserId(), request.SessionId, ct));
    }

    /// <summary>
    /// Pauses an in-progress workout session.
    /// </summary>
    /// <param name="sessionId">The session to pause.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The paused workout session.</returns>
    /// <response code="200">Workout paused successfully.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Workout session not found.</response>
    [HttpPost("{sessionId:guid}/pause")]
    public async Task<ActionResult<WorkoutSessionDto>> Pause(Guid sessionId, CancellationToken ct) =>
        Ok(await workoutService.PauseAsync(User.GetUserId(), sessionId, ct));

    /// <summary>
    /// Resumes a paused workout session.
    /// </summary>
    /// <param name="sessionId">The session to resume.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The resumed workout session.</returns>
    /// <response code="200">Workout resumed successfully.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Workout session not found.</response>
    [HttpPost("{sessionId:guid}/resume")]
    public async Task<ActionResult<WorkoutSessionDto>> Resume(Guid sessionId, CancellationToken ct) =>
        Ok(await workoutService.ResumeAsync(User.GetUserId(), sessionId, ct));

    /// <summary>
    /// Logs a completed set within an active workout session.
    /// </summary>
    /// <param name="sessionId">The workout session.</param>
    /// <param name="request">Set details such as exercise, reps, and weight.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The logged set record.</returns>
    /// <response code="200">Set logged successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Session, program, or exercise not found.</response>
    [HttpPost("{sessionId:guid}/sets")]
    public async Task<ActionResult<WorkoutSetLogDto>> LogSet(
        Guid sessionId, [FromBody] LogSetRequest request, CancellationToken ct)
    {
        var error = await logSetValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await workoutService.LogSetAsync(User.GetUserId(), sessionId, request, ct));
    }

    /// <summary>
    /// Updates an existing set log entry.
    /// </summary>
    /// <param name="setLogId">The set log to update.</param>
    /// <param name="request">Updated set values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated set record.</returns>
    /// <response code="200">Set updated successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Set log not found.</response>
    [HttpPut("sets/{setLogId:guid}")]
    public async Task<ActionResult<WorkoutSetLogDto>> UpdateSet(
        Guid setLogId, [FromBody] UpdateSetRequest request, CancellationToken ct)
    {
        var error = await updateSetValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await workoutService.UpdateSetAsync(User.GetUserId(), setLogId, request, ct));
    }

    /// <summary>
    /// Retrieves a single completed workout session from the user's history.
    /// </summary>
    /// <param name="sessionId">The session to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The workout session details.</returns>
    /// <response code="200">Session retrieved successfully.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Workout session not found.</response>
    [HttpGet("history/{sessionId:guid}")]
    public async Task<ActionResult<WorkoutSessionDto>> GetHistorySession(Guid sessionId, CancellationToken ct) =>
        Ok(await workoutService.GetHistorySessionAsync(User.GetUserId(), sessionId, ct));

    /// <summary>
    /// Returns a paginated list of the authenticated user's past workout sessions.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A page of workout session records.</returns>
    /// <response code="200">History retrieved successfully.</response>
    /// <response code="401">Authentication required.</response>
    [HttpGet("history")]
    public async Task<ActionResult<PagedResult<WorkoutSessionDto>>> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        return Ok(await workoutService.GetHistoryAsync(User.GetUserId(), page, pageSize, ct));
    }
}
