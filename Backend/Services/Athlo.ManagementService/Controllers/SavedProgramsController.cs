using Athlo.ManagementService.Services;
using Athlo.Models.DTOs.Programs;
using Athlo.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.ManagementService.Controllers;

[ApiController]
[Route("api/programs/saved")]
[Authorize]
[EnableRateLimiting("api")]
public class SavedProgramsController(ISavedProgramService savedProgramService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProgramListItemDto>>> GetSaved(CancellationToken ct) =>
        Ok(await savedProgramService.GetSavedAsync(User.GetUserId(), ct));

    [HttpGet("{programId:guid}")]
    public async Task<ActionResult<SavedProgramStatusDto>> GetStatus(Guid programId, CancellationToken ct)
    {
        var saved = await savedProgramService.IsSavedAsync(User.GetUserId(), programId, ct);
        return Ok(new SavedProgramStatusDto { ProgramId = programId, Saved = saved });
    }

    [HttpPost("{programId:guid}")]
    public async Task<IActionResult> Save(Guid programId, CancellationToken ct)
    {
        await savedProgramService.SaveAsync(User.GetUserId(), programId, ct);
        return NoContent();
    }

    [HttpDelete("{programId:guid}")]
    public async Task<IActionResult> Unsave(Guid programId, CancellationToken ct)
    {
        await savedProgramService.UnsaveAsync(User.GetUserId(), programId, ct);
        return NoContent();
    }
}

public class SavedProgramStatusDto
{
    public Guid ProgramId { get; set; }
    public bool Saved { get; set; }
}
