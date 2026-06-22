using Athlo.Database.DbContexts;
using Athlo.Models.Entities;
using Athlo.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Athlo.Repositories.Users;

public class UserRepository(AthloDbContext context) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        context.Users.AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public Task<int> CountAsync(UserRole? role = null, CancellationToken ct = default) =>
        role is null
            ? context.Users.CountAsync(ct)
            : context.Users.CountAsync(u => u.Role == role, ct);

    public async Task<IReadOnlyList<User>> GetByRolesAsync(IEnumerable<UserRole> roles, CancellationToken ct = default)
    {
        var roleSet = roles.ToHashSet();
        return await context.Users
            .AsNoTracking()
            .Where(u => roleSet.Contains(u.Role))
            .OrderBy(u => u.Role)
            .ThenBy(u => u.FullName)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = context.Users.AsNoTracking().OrderByDescending(u => u.CreatedAt);
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await context.Users.AddAsync(user, ct);

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        context.Users.Update(user);
        return Task.CompletedTask;
    }
}
