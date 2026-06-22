using Athlo.Mapper;
using Athlo.Models.DTOs.Programs;
using Athlo.Models.Entities;
using Athlo.Repositories;
using Athlo.Repositories.Categories;
using Athlo.Shared.Exceptions;

namespace Athlo.ManagementService.Services;

public class CategoryService(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var categories = await categoryRepository.GetAllAsync(ct);
        return categories.Select(ProgramMapper.ToCategory).ToList();
    }

    public async Task<CategoryDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var category = await categoryRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Category not found.");

        return ProgramMapper.ToCategory(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();

        if (await categoryRepository.SlugExistsAsync(slug, ct: ct))
            throw new ConflictException("A category with this slug already exists.");

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = slug,
            Icon = request.Icon.Trim()
        };

        await categoryRepository.AddAsync(category, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return ProgramMapper.ToCategory(category);
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var category = await categoryRepository.GetTrackedByIdAsync(id, ct)
            ?? throw new NotFoundException("Category not found.");

        var slug = request.Slug.Trim().ToLowerInvariant();

        if (await categoryRepository.SlugExistsAsync(slug, excludeId: id, ct: ct))
            throw new ConflictException("A category with this slug already exists.");

        category.Name = request.Name.Trim();
        category.Slug = slug;
        category.Icon = request.Icon.Trim();

        await unitOfWork.SaveChangesAsync(ct);

        return ProgramMapper.ToCategory(category);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await categoryRepository.GetTrackedByIdAsync(id, ct)
            ?? throw new NotFoundException("Category not found.");

        if (await categoryRepository.HasProgramsAsync(id, ct))
            throw new ConflictException("Cannot delete a category that has workout programs.");

        await categoryRepository.DeleteAsync(category, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
