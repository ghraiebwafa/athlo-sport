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
/// Manages workout program categories. Restricted to admin users.
/// </summary>
[ApiController]
[Route("api/admin/categories")]
[Authorize(Policy = AthloPolicies.AdminOrSuperAdmin)]
[EnableRateLimiting("api")]
public class AdminCategoriesController(
    ICategoryService categoryService,
    IValidator<CreateCategoryRequest> createValidator,
    IValidator<UpdateCategoryRequest> updateValidator) : ControllerBase
{
    /// <summary>
    /// Creates a new workout program category.
    /// </summary>
    /// <param name="request">Category name and metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created category.</returns>
    /// <response code="201">Category created successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="403">Caller is not an admin or super-admin.</response>
    /// <remarks>Requires the AdminOrSuperAdmin authorization policy.</remarks>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        var error = await createValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        var category = await categoryService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(ProgramsController.GetCategories), "Programs", null, category);
    }

    /// <summary>
    /// Updates an existing workout program category.
    /// </summary>
    /// <param name="id">The category's unique identifier.</param>
    /// <param name="request">Updated category values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated category.</returns>
    /// <response code="200">Category updated successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="403">Caller is not an admin or super-admin.</response>
    /// <response code="404">Category not found.</response>
    /// <remarks>Requires the AdminOrSuperAdmin authorization policy.</remarks>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        var error = await updateValidator.ToValidationErrorAsync(request, HttpContext, ct);
        if (error is not null) return (ActionResult)error;

        return Ok(await categoryService.UpdateAsync(id, request, ct));
    }

    /// <summary>
    /// Deletes a workout program category.
    /// </summary>
    /// <param name="id">The category's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Category deleted successfully.</response>
    /// <response code="403">Caller is not an admin or super-admin.</response>
    /// <response code="404">Category not found.</response>
    /// <remarks>Requires the AdminOrSuperAdmin authorization policy.</remarks>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await categoryService.DeleteAsync(id, ct);
        return NoContent();
    }
}
