namespace Athlo.Models.Entities;

public class UserAchievement
{
    public Guid UserId { get; set; }
    public string AchievementKey { get; set; } = string.Empty;
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
