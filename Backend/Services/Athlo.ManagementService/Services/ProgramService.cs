using Athlo.Mapper;
using Athlo.Models.DTOs.Programs;
using Athlo.Models.Entities;
using Athlo.Repositories;
using Athlo.Repositories.Categories;
using Athlo.Repositories.Programs;
using Athlo.Shared.Exceptions;

namespace Athlo.ManagementService.Services;

public class ProgramService(
    IProgramRepository programRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : IProgramService
{
    public async Task<IReadOnlyList<ProgramListItemDto>> GetAllAsync(CancellationToken ct = default)
    {
        var programs = await programRepository.GetAllAsync(ct);
        return programs.Select(ProgramMapper.ToListItem).ToList();
    }

    public async Task<ProgramDetailDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var program = await programRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Workout program not found.");

        return ProgramMapper.ToDetail(program);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var categories = await categoryRepository.GetAllAsync(ct);
        return categories.Select(ProgramMapper.ToCategory).ToList();
    }

    public async Task<ProgramDetailDto> CreateAsync(CreateProgramRequest request, CancellationToken ct = default)
    {
        await ValidateProgramRequestAsync(request.CategoryId, request.Exercises, ct);

        var program = new WorkoutProgram
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            DurationMinutes = request.DurationMinutes,
            Difficulty = request.Difficulty,
            EstimatedCalories = request.EstimatedCalories,
            ImageUrl = request.ImageUrl?.Trim(),
            IsFeatured = request.IsFeatured,
            CategoryId = request.CategoryId,
            ProgramExercises = BuildProgramExercises(Guid.Empty, request.Exercises)
        };

        foreach (var exercise in program.ProgramExercises)
            exercise.ProgramId = program.Id;

        await programRepository.AddAsync(program, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var created = await programRepository.GetByIdAsync(program.Id, ct);
        return ProgramMapper.ToDetail(created!);
    }

    public async Task<ProgramDetailDto> UpdateAsync(Guid id, UpdateProgramRequest request, CancellationToken ct = default)
    {
        await ValidateProgramRequestAsync(request.CategoryId, request.Exercises, ct);

        var program = await programRepository.GetTrackedByIdAsync(id, ct)
            ?? throw new NotFoundException("Workout program not found.");

        program.Name = request.Name.Trim();
        program.Description = request.Description.Trim();
        program.DurationMinutes = request.DurationMinutes;
        program.Difficulty = request.Difficulty;
        program.EstimatedCalories = request.EstimatedCalories;
        program.ImageUrl = request.ImageUrl?.Trim();
        program.IsFeatured = request.IsFeatured;
        program.CategoryId = request.CategoryId;

        program.ProgramExercises.Clear();
        foreach (var exercise in BuildProgramExercises(program.Id, request.Exercises))
            program.ProgramExercises.Add(exercise);

        await programRepository.UpdateAsync(program, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var updated = await programRepository.GetByIdAsync(id, ct);
        return ProgramMapper.ToDetail(updated!);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var program = await programRepository.GetTrackedByIdAsync(id, ct)
            ?? throw new NotFoundException("Workout program not found.");

        if (await programRepository.HasWorkoutSessionsAsync(id, ct))
            throw new ConflictException("Cannot delete a program that has workout history.");

        await programRepository.DeleteAsync(program, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task ValidateProgramRequestAsync(
        Guid categoryId,
        IReadOnlyList<ProgramExerciseInput> exercises,
        CancellationToken ct)
    {
        if (await categoryRepository.GetByIdAsync(categoryId, ct) is null)
            throw new NotFoundException("Category not found.");

        if (exercises.Count == 0)
            throw new AppException("At least one exercise is required.", 400);

        if (exercises.Select(e => e.OrderIndex).Distinct().Count() != exercises.Count)
            throw new AppException("Exercise order indexes must be unique.", 400);

        var exerciseIds = exercises.Select(e => e.ExerciseId).ToList();
        if (!await programRepository.AllExercisesExistAsync(exerciseIds, ct))
            throw new NotFoundException("One or more exercises were not found.");
    }

    private static List<ProgramExercise> BuildProgramExercises(Guid programId, IReadOnlyList<ProgramExerciseInput> exercises) =>
        exercises
            .OrderBy(e => e.OrderIndex)
            .Select(e => new ProgramExercise
            {
                Id = Guid.NewGuid(),
                ProgramId = programId,
                ExerciseId = e.ExerciseId,
                OrderIndex = e.OrderIndex,
                Sets = e.Sets,
                Reps = e.Reps,
                DurationSeconds = e.DurationSeconds
            })
            .ToList();
}
