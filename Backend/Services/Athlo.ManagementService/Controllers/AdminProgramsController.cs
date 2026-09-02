using Athlo.ManagementService.Services;
using Athlo.Models.DTOs.Programs;
using Athlo.Shared.Authorization;
using Athlo.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.ManagementService.Controllers;

/// <summary>
/// Manages workout programs. Restricted to admin users.
/// </summary>
[ApiController]
[Route("api/admin/programs")]
[Authorize(Policy = AthloPolicies.AdminOrSuperAdmin)]
[EnableRateLimiting("api")]
public class AdminProgramsController(
    IProgramService programService,
    IValidator<CreateProgramRequest> createValidator,
    IValidator<UpdateProgramRequest> updateValidator) : ControllerBase
{
    /// <summary>
    /// Creates a new workout program.
    /// </summary>
    /// <param name="request">Program details including exercises and metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created program.</returns>
    /// <response code="201">Program created successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="403">Caller is not an admin or super-admin.</response>
    /// <remarks>Requires the AdminOrSuperAdmin authorization policy.</remarks>
    [HttpPost]
    public async Task<ActionResult<ProgramDetailDto>> Create([FromBody] CreateProgramRequest request, CancellationToken ct)
    {
        var error = await createValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        var program = await programService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(ProgramsController.GetById), "Programs", new { id = program.Id }, program);
    }

    /// <summary>
    /// Updates an existing workout program.
    /// </summary>
    /// <param name="id">The program's unique identifier.</param>
    /// <param name="request">Updated program values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated program.</returns>
    /// <response code="200">Program updated successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="403">Caller is not an admin or super-admin.</response>
    /// <response code="404">Program not found.</response>
    /// <remarks>Requires the AdminOrSuperAdmin authorization policy.</remarks>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProgramDetailDto>> Update(Guid id, [FromBody] UpdateProgramRequest request, CancellationToken ct)
    {
        var error = await updateValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await programService.UpdateAsync(id, request, ct));
    }

    /// <summary>
    /// Deletes a workout program.
    /// </summary>
    /// <param name="id">The program's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Program deleted successfully.</response>
    /// <response code="403">Caller is not an admin or super-admin.</response>
    /// <response code="404">Program not found.</response>
    /// <remarks>Requires the AdminOrSuperAdmin authorization policy.</remarks>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await programService.DeleteAsync(id, ct);
        return NoContent();
    }
}
