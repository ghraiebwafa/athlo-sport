using Athlo.ManagementService.Services;
using Athlo.Models.DTOs.Exercises;
using Athlo.Shared.Authorization;
using Athlo.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.ManagementService.Controllers;

[ApiController]
[Route("api/admin/exercises")]
[Authorize(Policy = AthloPolicies.AdminOrSuperAdmin)]
[EnableRateLimiting("api")]
public class AdminExercisesController(
    IExerciseService exerciseService,
    IValidator<CreateExerciseRequest> createValidator,
    IValidator<UpdateExerciseRequest> updateValidator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ExerciseDto>> Create([FromBody] CreateExerciseRequest request, CancellationToken ct)
    {
        var error = await createValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        var exercise = await exerciseService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(ExercisesController.GetById), "Exercises", new { id = exercise.Id }, exercise);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExerciseDto>> Update(Guid id, [FromBody] UpdateExerciseRequest request, CancellationToken ct)
    {
        var error = await updateValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await exerciseService.UpdateAsync(id, request, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await exerciseService.DeleteAsync(id, ct);
        return NoContent();
    }
}
