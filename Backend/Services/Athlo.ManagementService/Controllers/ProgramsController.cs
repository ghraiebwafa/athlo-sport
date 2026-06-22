using Athlo.ManagementService.Services;
using Athlo.Models.DTOs.Programs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.ManagementService.Controllers;

[ApiController]
[Route("api/programs")]
[EnableRateLimiting("api")]
public class ProgramsController(IProgramService programService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ProgramListItemDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await programService.GetAllAsync(ct));
    }

    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetCategories(CancellationToken ct)
    {
        return Ok(await programService.GetCategoriesAsync(ct));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProgramDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await programService.GetByIdAsync(id, ct));
    }
}
