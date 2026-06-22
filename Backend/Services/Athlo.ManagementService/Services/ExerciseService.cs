using Athlo.Mapper;
using Athlo.Models.DTOs.Exercises;
using Athlo.Models.Entities;
using Athlo.Repositories;
using Athlo.Repositories.Exercises;
using Athlo.Shared.Exceptions;

namespace Athlo.ManagementService.Services;

public class ExerciseService(IExerciseRepository exerciseRepository, IUnitOfWork unitOfWork) : IExerciseService
{
    public async Task<IReadOnlyList<ExerciseDto>> GetAllAsync(CancellationToken ct = default)
    {
        var exercises = await exerciseRepository.GetAllAsync(ct);
        return exercises.Select(ExerciseMapper.ToDto).ToList();
    }

    public async Task<ExerciseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var exercise = await exerciseRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Exercise not found.");

        return ExerciseMapper.ToDto(exercise);
    }

    public async Task<ExerciseDto> CreateAsync(CreateExerciseRequest request, CancellationToken ct = default)
    {
        var exercise = new Exercise
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            ImageUrl = request.ImageUrl?.Trim()
        };

        await exerciseRepository.AddAsync(exercise, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return ExerciseMapper.ToDto(exercise);
    }

    public async Task<ExerciseDto> UpdateAsync(Guid id, UpdateExerciseRequest request, CancellationToken ct = default)
    {
        var exercise = await exerciseRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Exercise not found.");

        exercise.Name = request.Name.Trim();
        exercise.ImageUrl = request.ImageUrl?.Trim();

        await exerciseRepository.UpdateAsync(exercise, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return ExerciseMapper.ToDto(exercise);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var exercise = await exerciseRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Exercise not found.");

        if (await exerciseRepository.IsUsedInProgramsAsync(id, ct))
            throw new ConflictException("Cannot delete an exercise that is used in workout programs.");

        await exerciseRepository.DeleteAsync(exercise, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
