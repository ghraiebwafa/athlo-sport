using Athlo.Mapper;
using Athlo.Models.DTOs.Programs;
using Athlo.Repositories;
using Athlo.Repositories.Programs;
using Athlo.Shared.Exceptions;

namespace Athlo.ManagementService.Services;

public interface ISavedProgramService
{
    Task<IReadOnlyList<ProgramListItemDto>> GetSavedAsync(Guid userId, CancellationToken ct = default);
    Task<bool> IsSavedAsync(Guid userId, Guid programId, CancellationToken ct = default);
    Task SaveAsync(Guid userId, Guid programId, CancellationToken ct = default);
    Task UnsaveAsync(Guid userId, Guid programId, CancellationToken ct = default);
}

public class SavedProgramService(
    ISavedProgramRepository savedProgramRepository,
    IProgramRepository programRepository,
    IUnitOfWork unitOfWork,
    ILogger<SavedProgramService> logger) : ISavedProgramService
{
    public async Task<IReadOnlyList<ProgramListItemDto>> GetSavedAsync(Guid userId, CancellationToken ct = default)
    {
        var ids = await savedProgramRepository.GetSavedProgramIdsAsync(userId, ct);
        if (ids.Count == 0)
            return [];

        var programs = await programRepository.GetAllAsync(ct);
        var byId = programs.ToDictionary(p => p.Id);
        return ids
            .Where(byId.ContainsKey)
            .Select(id => ProgramMapper.ToListItem(byId[id]))
            .ToList();
    }

    public Task<bool> IsSavedAsync(Guid userId, Guid programId, CancellationToken ct = default) =>
        savedProgramRepository.IsSavedAsync(userId, programId, ct);

    public async Task SaveAsync(Guid userId, Guid programId, CancellationToken ct = default)
    {
        _ = await programRepository.GetByIdAsync(programId, ct)
            ?? throw new NotFoundException("Workout program not found.");

        await savedProgramRepository.SaveAsync(userId, programId, ct);
        await unitOfWork.SaveChangesAsync(ct);
        logger.LogInformation("Program saved UserId={UserId} ProgramId={ProgramId}", userId, programId);
    }

    public async Task UnsaveAsync(Guid userId, Guid programId, CancellationToken ct = default)
    {
        if (!await savedProgramRepository.RemoveAsync(userId, programId, ct))
            throw new NotFoundException("Saved program not found.");

        await unitOfWork.SaveChangesAsync(ct);
        logger.LogInformation("Program unsaved UserId={UserId} ProgramId={ProgramId}", userId, programId);
    }
}
