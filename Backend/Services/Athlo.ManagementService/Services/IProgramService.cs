using Athlo.Models.DTOs.Programs;

namespace Athlo.ManagementService.Services;

public interface IProgramService
{
    Task<IReadOnlyList<ProgramListItemDto>> GetAllAsync(CancellationToken ct = default);
    Task<ProgramDetailDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default);
    Task<ProgramDetailDto> CreateAsync(CreateProgramRequest request, CancellationToken ct = default);
    Task<ProgramDetailDto> UpdateAsync(Guid id, UpdateProgramRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
