using Athlo.ManagementService.Services;
using Athlo.Models.DTOs.Exercises;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.ManagementService.Controllers;

/// <summary>
/// Provides public read access to the exercise catalog.
/// </summary>
[ApiController]
[Route("api/exercises")]
[EnableRateLimiting("api")]
public class ExercisesController(IExerciseService exerciseService) : ControllerBase
{
    /// <summary>
    /// Returns all exercises in the catalog.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of exercise records.</returns>
    /// <response code="200">Exercises retrieved successfully.</response>
    /// <remarks>Anonymous endpoint.</remarks>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ExerciseDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await exerciseService.GetAllAsync(ct));
    }

    /// <summary>
    /// Retrieves a single exercise by ID.
    /// </summary>
    /// <param name="id">The exercise's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The exercise details.</returns>
    /// <response code="200">Exercise retrieved successfully.</response>
    /// <response code="404">Exercise not found.</response>
    /// <remarks>Anonymous endpoint.</remarks>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ExerciseDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await exerciseService.GetByIdAsync(id, ct));
    }
}
