using Athlo.ManagementService.Services;
using Athlo.Models.DTOs.Exercises;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.ManagementService.Controllers;

[ApiController]
[Route("api/exercises")]
[EnableRateLimiting("api")]
public class ExercisesController(IExerciseService exerciseService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ExerciseDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await exerciseService.GetAllAsync(ct));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ExerciseDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await exerciseService.GetByIdAsync(id, ct));
    }
}
