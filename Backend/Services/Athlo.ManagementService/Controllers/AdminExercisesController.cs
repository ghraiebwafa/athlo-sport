using Athlo.ManagementService.Services;
using Athlo.Models.DTOs.Exercises;
using Athlo.Shared.Authorization;
using Athlo.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.ManagementService.Controllers;

/// <summary>
/// Manages the exercise catalog. Restricted to admin users.
/// </summary>
[ApiController]
[Route("api/admin/exercises")]
[Authorize(Policy = AthloPolicies.AdminOrSuperAdmin)]
[EnableRateLimiting("api")]
public class AdminExercisesController(
    IExerciseService exerciseService,
    IValidator<CreateExerciseRequest> createValidator,
    IValidator<UpdateExerciseRequest> updateValidator) : ControllerBase
{
    /// <summary>
    /// Creates a new exercise in the catalog.
    /// </summary>
    /// <param name="request">Exercise name, description, and metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created exercise.</returns>
    /// <response code="201">Exercise created successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="403">Caller is not an admin or super-admin.</response>
    /// <remarks>Requires the AdminOrSuperAdmin authorization policy.</remarks>
    [HttpPost]
    public async Task<ActionResult<ExerciseDto>> Create([FromBody] CreateExerciseRequest request, CancellationToken ct)
    {
        var error = await createValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        var exercise = await exerciseService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(ExercisesController.GetById), "Exercises", new { id = exercise.Id }, exercise);
    }

    /// <summary>
    /// Updates an existing exercise in the catalog.
    /// </summary>
    /// <param name="id">The exercise's unique identifier.</param>
    /// <param name="request">Updated exercise values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated exercise.</returns>
    /// <response code="200">Exercise updated successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="403">Caller is not an admin or super-admin.</response>
    /// <response code="404">Exercise not found.</response>
    /// <remarks>Requires the AdminOrSuperAdmin authorization policy.</remarks>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExerciseDto>> Update(Guid id, [FromBody] UpdateExerciseRequest request, CancellationToken ct)
    {
        var error = await updateValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await exerciseService.UpdateAsync(id, request, ct));
    }

    /// <summary>
    /// Deletes an exercise from the catalog.
    /// </summary>
    /// <param name="id">The exercise's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Exercise deleted successfully.</response>
    /// <response code="403">Caller is not an admin or super-admin.</response>
    /// <response code="404">Exercise not found.</response>
    /// <remarks>Requires the AdminOrSuperAdmin authorization policy.</remarks>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await exerciseService.DeleteAsync(id, ct);
        return NoContent();
    }
}
