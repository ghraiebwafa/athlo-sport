using Athlo.Models.DTOs.Exercises;

namespace Athlo.ManagementService.Services;

public interface IExerciseService
{
    Task<IReadOnlyList<ExerciseDto>> GetAllAsync(CancellationToken ct = default);
    Task<ExerciseDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ExerciseDto> CreateAsync(CreateExerciseRequest request, CancellationToken ct = default);
    Task<ExerciseDto> UpdateAsync(Guid id, UpdateExerciseRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
