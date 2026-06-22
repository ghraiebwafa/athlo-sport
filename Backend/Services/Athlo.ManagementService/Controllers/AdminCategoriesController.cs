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
[Route("api/admin/categories")]
[Authorize(Policy = AthloPolicies.AdminOrSuperAdmin)]
[EnableRateLimiting("api")]
public class AdminCategoriesController(
    ICategoryService categoryService,
    IValidator<CreateCategoryRequest> createValidator,
    IValidator<UpdateCategoryRequest> updateValidator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        var error = await createValidator.ToValidationErrorAsync(request, ct);
        if (error is not null) return (ActionResult)error;

        var category = await categoryService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(ProgramsController.GetCategories), "Programs", null, category);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        var error = await updateValidator.ToValidationErrorAsync(request, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await categoryService.UpdateAsync(id, request, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await categoryService.DeleteAsync(id, ct);
        return NoContent();
    }
}
