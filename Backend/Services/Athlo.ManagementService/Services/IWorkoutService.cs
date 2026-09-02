using Athlo.Models.DTOs.Workouts;

namespace Athlo.ManagementService.Services;

/// <summary>
/// Workout session lifecycle: start, pause, resume, complete, cancel, set logging, and history.
/// All user-scoped operations enforce ownership; cross-user access returns 404 to prevent resource enumeration.
/// </summary>
public interface IWorkoutService
{
    /// <summary>
    /// Returns the authenticated user's currently in-progress workout session, if any.
    /// </summary>
    /// <param name="userId">Authenticated user identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The active session, or <c>null</c> when none is in progress.</returns>
    Task<WorkoutSessionDto?> GetActiveAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Starts a new workout session for the given program.
    /// </summary>
    /// <param name="userId">Authenticated user identifier.</param>
    /// <param name="programId">Workout program to execute.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created session.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.NotFoundException">Thrown when the program does not exist.</exception>
    /// <exception cref="Athlo.Shared.Exceptions.ConflictException">
    /// Thrown when the user already has an active session.
    /// </exception>
    Task<WorkoutSessionDto> StartAsync(Guid userId, Guid programId, CancellationToken ct = default);

    /// <summary>
    /// Marks an in-progress session as completed and records calories burned.
    /// </summary>
    /// <param name="userId">Authenticated user identifier.</param>
    /// <param name="sessionId">Session to complete.</param>
    /// <param name="caloriesBurned">Total calories reported for the session.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The completed session.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.NotFoundException">
    /// Thrown when the session does not exist or belongs to another user (404, not 403).
    /// </exception>
    /// <exception cref="Athlo.Shared.Exceptions.ConflictException">
    /// Thrown when the session is not in the InProgress state.
    /// </exception>
    Task<WorkoutSessionDto> CompleteAsync(Guid userId, Guid sessionId, int caloriesBurned, CancellationToken ct = default);

    /// <summary>
    /// Cancels an in-progress session without recording completion.
    /// </summary>
    /// <param name="userId">Authenticated user identifier.</param>
    /// <param name="sessionId">Session to cancel.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The cancelled session.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.NotFoundException">
    /// Thrown when the session does not exist or belongs to another user (404, not 403).
    /// </exception>
    /// <exception cref="Athlo.Shared.Exceptions.ConflictException">
    /// Thrown when the session is not in the InProgress state.
    /// </exception>
    Task<WorkoutSessionDto> CancelAsync(Guid userId, Guid sessionId, CancellationToken ct = default);

    /// <summary>
    /// Pauses an in-progress session.
    /// </summary>
    /// <param name="userId">Authenticated user identifier.</param>
    /// <param name="sessionId">Session to pause.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The paused session.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.NotFoundException">
    /// Thrown when the session does not exist or belongs to another user (404, not 403).
    /// </exception>
    /// <exception cref="Athlo.Shared.Exceptions.ConflictException">
    /// Thrown when the session is not in the InProgress state.
    /// </exception>
    Task<WorkoutSessionDto> PauseAsync(Guid userId, Guid sessionId, CancellationToken ct = default);

    /// <summary>
    /// Resumes a paused in-progress session.
    /// </summary>
    /// <param name="userId">Authenticated user identifier.</param>
    /// <param name="sessionId">Session to resume.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The resumed session.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.NotFoundException">
    /// Thrown when the session does not exist or belongs to another user (404, not 403).
    /// </exception>
    /// <exception cref="Athlo.Shared.Exceptions.ConflictException">
    /// Thrown when the session is not in the InProgress state.
    /// </exception>
    Task<WorkoutSessionDto> ResumeAsync(Guid userId, Guid sessionId, CancellationToken ct = default);

    /// <summary>
    /// Records a set completion during an in-progress session.
    /// </summary>
    /// <param name="userId">Authenticated user identifier.</param>
    /// <param name="sessionId">Active session identifier.</param>
    /// <param name="request">Set details including exercise, set number, reps, and weight.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The persisted set log.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.NotFoundException">
    /// Thrown when the session, program, or exercise does not exist, or the session belongs
    /// to another user (404, not 403).
    /// </exception>
    /// <exception cref="Athlo.Shared.Exceptions.ConflictException">
    /// Thrown when the session is not in the InProgress state.
    /// </exception>
    Task<WorkoutSetLogDto> LogSetAsync(Guid userId, Guid sessionId, LogSetRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing set log on an in-progress session.
    /// </summary>
    /// <param name="userId">Authenticated user identifier.</param>
    /// <param name="setLogId">Set log to update.</param>
    /// <param name="request">Updated reps, weight, and completion flag.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated set log.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.NotFoundException">
    /// Thrown when the set log does not exist or belongs to another user's session (404, not 403).
    /// </exception>
    /// <exception cref="Athlo.Shared.Exceptions.ConflictException">
    /// Thrown when the parent session is not in the InProgress state.
    /// </exception>
    Task<WorkoutSetLogDto> UpdateSetAsync(Guid userId, Guid setLogId, UpdateSetRequest request, CancellationToken ct = default);

    /// <summary>
    /// Returns a paginated list of the authenticated user's completed workout sessions.
    /// </summary>
    /// <param name="userId">Authenticated user identifier.</param>
    /// <param name="page">One-based page number (clamped to at least 1).</param>
    /// <param name="pageSize">Page size (clamped between 1 and 50).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paged completed sessions.</returns>
    Task<PagedResult<WorkoutSessionDto>> GetHistoryAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Returns a single completed session from the authenticated user's history.
    /// </summary>
    /// <param name="userId">Authenticated user identifier.</param>
    /// <param name="sessionId">Completed session identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The session detail.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.NotFoundException">
    /// Thrown when the session does not exist, is not completed, or belongs to another user.
    /// </exception>
    Task<WorkoutSessionDto> GetHistorySessionAsync(Guid userId, Guid sessionId, CancellationToken ct = default);

    /// <summary>
    /// Cancels in-progress sessions that have been abandoned longer than the given age.
    /// Intended for background cleanup, not user-facing requests.
    /// </summary>
    /// <param name="maxAge">Maximum duration a session may remain in progress before cancellation.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The number of sessions cancelled.</returns>
    Task<int> CancelStaleSessionsAsync(TimeSpan maxAge, CancellationToken ct = default);
}
