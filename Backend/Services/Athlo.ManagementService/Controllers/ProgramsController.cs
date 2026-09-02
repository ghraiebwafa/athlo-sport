using Athlo.ManagementService.Services;
using Athlo.Models.DTOs.Programs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.ManagementService.Controllers;

/// <summary>
/// Provides public read access to workout programs and categories.
/// </summary>
[ApiController]
[Route("api/programs")]
[EnableRateLimiting("api")]
public class ProgramsController(IProgramService programService) : ControllerBase
{
    /// <summary>
    /// Returns all available workout programs.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of program summaries.</returns>
    /// <response code="200">Programs retrieved successfully.</response>
    /// <remarks>Anonymous endpoint.</remarks>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ProgramListItemDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await programService.GetAllAsync(ct));
    }

    /// <summary>
    /// Returns all workout program categories.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of categories.</returns>
    /// <response code="200">Categories retrieved successfully.</response>
    /// <remarks>Anonymous endpoint.</remarks>
    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetCategories(CancellationToken ct)
    {
        return Ok(await programService.GetCategoriesAsync(ct));
    }

    /// <summary>
    /// Retrieves full details for a single workout program.
    /// </summary>
    /// <param name="id">The program's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The program details including exercises.</returns>
    /// <response code="200">Program retrieved successfully.</response>
    /// <response code="404">Program not found.</response>
    /// <remarks>Anonymous endpoint.</remarks>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProgramDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await programService.GetByIdAsync(id, ct));
    }
}
