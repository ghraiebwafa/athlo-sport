using Athlo.Models.DTOs.Achievements;
using Athlo.Repositories;
using Athlo.Repositories.Achievements;
using Athlo.Repositories.Workouts;
using Athlo.Shared.Helpers;

namespace Athlo.ManagementService.Services;

public interface IAchievementService
{
    Task<IReadOnlyList<AchievementDto>> GetForUserAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<AchievementDto>> EvaluateAndUnlockAsync(Guid userId, CancellationToken ct = default);
}

public class AchievementService(
    IUserAchievementRepository achievementRepository,
    IWorkoutSessionRepository sessionRepository,
    IUnitOfWork unitOfWork,
    ILogger<AchievementService> logger) : IAchievementService
{
    public async Task<IReadOnlyList<AchievementDto>> GetForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var unlocked = await achievementRepository.GetForUserAsync(userId, ct);
        var unlockedMap = unlocked.ToDictionary(a => a.AchievementKey, a => a.UnlockedAt);
        var snapshot = await BuildSnapshotAsync(userId, ct);

        return AchievementCatalog.All.Select(def =>
        {
            unlockedMap.TryGetValue(def.Key, out var at);
            var isUnlocked = unlockedMap.ContainsKey(def.Key) || def.IsMet(snapshot);
            return new AchievementDto
            {
                Key = def.Key,
                Title = def.Title,
                Subtitle = def.Subtitle,
                Color = def.Color,
                Unlocked = isUnlocked,
                UnlockedAt = unlockedMap.ContainsKey(def.Key) ? at : null
            };
        }).ToList();
    }

    public async Task<IReadOnlyList<AchievementDto>> EvaluateAndUnlockAsync(Guid userId, CancellationToken ct = default)
    {
        var snapshot = await BuildSnapshotAsync(userId, ct);
        var newlyUnlocked = new List<AchievementDto>();

        foreach (var def in AchievementCatalog.All)
        {
            if (!def.IsMet(snapshot))
                continue;
            if (await achievementRepository.HasAsync(userId, def.Key, ct))
                continue;

            await achievementRepository.UnlockAsync(userId, def.Key, ct);
            newlyUnlocked.Add(new AchievementDto
            {
                Key = def.Key,
                Title = def.Title,
                Subtitle = def.Subtitle,
                Color = def.Color,
                Unlocked = true,
                UnlockedAt = DateTime.UtcNow
            });
            logger.LogInformation("Achievement unlocked UserId={UserId} Key={Key}", userId, def.Key);
        }

        if (newlyUnlocked.Count > 0)
            await unitOfWork.SaveChangesAsync(ct);

        return newlyUnlocked;
    }

    private async Task<AchievementProgressSnapshot> BuildSnapshotAsync(Guid userId, CancellationToken ct)
    {
        var (totalCount, totalCalories) = await sessionRepository.GetCompletedAggregatesAsync(userId, ct);
        var dates = await sessionRepository.GetCompletedDatesAsync(userId, ct);
        var streak = StreakCalculator.Calculate(dates);
        return new AchievementProgressSnapshot(totalCount, totalCalories, streak);
    }
}
