using Athlo.ManagementService.Services;
using Athlo.Models.DTOs.Programs;
using Athlo.Shared.Authorization;
using Athlo.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.ManagementService.Controllers;

[ApiController]
[Route("api/admin/programs")]
[Authorize(Policy = AthloPolicies.AdminOrSuperAdmin)]
[EnableRateLimiting("api")]
public class AdminProgramsController(
    IProgramService programService,
    IValidator<CreateProgramRequest> createValidator,
    IValidator<UpdateProgramRequest> updateValidator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ProgramDetailDto>> Create([FromBody] CreateProgramRequest request, CancellationToken ct)
    {
        var error = await createValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        var program = await programService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(ProgramsController.GetById), "Programs", new { id = program.Id }, program);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProgramDetailDto>> Update(Guid id, [FromBody] UpdateProgramRequest request, CancellationToken ct)
    {
        var error = await updateValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await programService.UpdateAsync(id, request, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await programService.DeleteAsync(id, ct);
        return NoContent();
    }
}
