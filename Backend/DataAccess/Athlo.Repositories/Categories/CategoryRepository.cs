using Athlo.Database.DbContexts;
using Athlo.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Athlo.Repositories.Categories;

public class CategoryRepository(AthloDbContext context) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default) =>
        await context.Categories
            .AsNoTracking()
            .Include(c => c.Programs)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Categories
            .AsNoTracking()
            .Include(c => c.Programs)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Category?> GetTrackedByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default) =>
        context.Categories.AnyAsync(
            c => c.Slug == slug && (excludeId == null || c.Id != excludeId),
            ct);

    public Task<bool> HasProgramsAsync(Guid categoryId, CancellationToken ct = default) =>
        context.WorkoutPrograms.AnyAsync(p => p.CategoryId == categoryId, ct);

    public async Task AddAsync(Category category, CancellationToken ct = default) =>
        await context.Categories.AddAsync(category, ct);

    public Task UpdateAsync(Category category, CancellationToken ct = default)
    {
        context.Categories.Update(category);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Category category, CancellationToken ct = default)
    {
        context.Categories.Remove(category);
        return Task.CompletedTask;
    }
}
