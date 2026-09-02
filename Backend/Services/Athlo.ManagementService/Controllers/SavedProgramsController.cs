using Athlo.ManagementService.Services;
using Athlo.Models.DTOs.Programs;
using Athlo.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athlo.ManagementService.Controllers;

/// <summary>
/// Manages the authenticated user's saved (bookmarked) workout programs.
/// </summary>
[ApiController]
[Route("api/programs/saved")]
[Authorize]
[EnableRateLimiting("api")]
public class SavedProgramsController(ISavedProgramService savedProgramService) : ControllerBase
{
    /// <summary>
    /// Returns all programs saved by the authenticated user.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of saved program summaries.</returns>
    /// <response code="200">Saved programs retrieved successfully.</response>
    /// <response code="401">Authentication required.</response>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProgramListItemDto>>> GetSaved(CancellationToken ct) =>
        Ok(await savedProgramService.GetSavedAsync(User.GetUserId(), ct));

    /// <summary>
    /// Checks whether a specific program is saved by the authenticated user.
    /// </summary>
    /// <param name="programId">The program to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The saved status for the given program.</returns>
    /// <response code="200">Status retrieved successfully.</response>
    /// <response code="401">Authentication required.</response>
    [HttpGet("{programId:guid}")]
    public async Task<ActionResult<SavedProgramStatusDto>> GetStatus(Guid programId, CancellationToken ct)
    {
        var saved = await savedProgramService.IsSavedAsync(User.GetUserId(), programId, ct);
        return Ok(new SavedProgramStatusDto { ProgramId = programId, Saved = saved });
    }

    /// <summary>
    /// Saves a program to the authenticated user's list.
    /// </summary>
    /// <param name="programId">The program to save.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Program saved successfully.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Program not found.</response>
    /// <remarks>Idempotent — saving an already-saved program succeeds without error.</remarks>
    [HttpPost("{programId:guid}")]
    public async Task<IActionResult> Save(Guid programId, CancellationToken ct)
    {
        await savedProgramService.SaveAsync(User.GetUserId(), programId, ct);
        return NoContent();
    }

    /// <summary>
    /// Removes a program from the authenticated user's saved list.
    /// </summary>
    /// <param name="programId">The program to unsave.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Program unsaved successfully.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Saved program not found.</response>
    /// <remarks>Returns 404 if the program was not in the user's saved list.</remarks>
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
