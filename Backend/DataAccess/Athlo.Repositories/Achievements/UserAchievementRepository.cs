using Athlo.Database.DbContexts;
using Athlo.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Athlo.Repositories.Achievements;

public interface IUserAchievementRepository
{
    Task<IReadOnlyList<UserAchievement>> GetForUserAsync(Guid userId, CancellationToken ct = default);
    Task<bool> HasAsync(Guid userId, string key, CancellationToken ct = default);
    Task UnlockAsync(Guid userId, string key, CancellationToken ct = default);
}

public class UserAchievementRepository(AthloDbContext context) : IUserAchievementRepository
{
    public async Task<IReadOnlyList<UserAchievement>> GetForUserAsync(Guid userId, CancellationToken ct = default) =>
        await context.UserAchievements
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.UnlockedAt)
            .ToListAsync(ct);

    public Task<bool> HasAsync(Guid userId, string key, CancellationToken ct = default) =>
        context.UserAchievements.AnyAsync(a => a.UserId == userId && a.AchievementKey == key, ct);

    public async Task UnlockAsync(Guid userId, string key, CancellationToken ct = default)
    {
        if (await HasAsync(userId, key, ct))
            return;

        await context.UserAchievements.AddAsync(new UserAchievement
        {
            UserId = userId,
            AchievementKey = key,
            UnlockedAt = DateTime.UtcNow
        }, ct);
    }
}
