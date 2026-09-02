using Athlo.Models.DTOs.Programs;

namespace Athlo.ManagementService.Services;

/// <summary>
/// CRUD operations for workout programs and read access to program categories.
/// Program data is shared across users; mutating operations require admin authorization at the API layer.
/// </summary>
public interface IProgramService
{
    /// <summary>
    /// Returns all workout programs ordered for display.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Program list items.</returns>
    Task<IReadOnlyList<ProgramListItemDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns full detail for a single workout program.
    /// </summary>
    /// <param name="id">Program identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Program detail including exercises.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.NotFoundException">Thrown when the program does not exist.</exception>
    Task<ProgramDetailDto> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Returns all workout program categories.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Category list.</returns>
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default);

    /// <summary>
    /// Creates a new workout program with exercises.
    /// </summary>
    /// <param name="request">Program definition.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created program detail.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.NotFoundException">
    /// Thrown when a referenced category or exercise does not exist.
    /// </exception>
    Task<ProgramDetailDto> CreateAsync(CreateProgramRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing workout program.
    /// </summary>
    /// <param name="id">Program identifier.</param>
    /// <param name="request">Updated program fields and exercise list.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated program detail.</returns>
    /// <exception cref="Athlo.Shared.Exceptions.NotFoundException">
    /// Thrown when the program, a referenced category, or an exercise does not exist.
    /// </exception>
    Task<ProgramDetailDto> UpdateAsync(Guid id, UpdateProgramRequest request, CancellationToken ct = default);

    /// <summary>
    /// Permanently deletes a workout program.
    /// </summary>
    /// <param name="id">Program identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Athlo.Shared.Exceptions.NotFoundException">Thrown when the program does not exist.</exception>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
