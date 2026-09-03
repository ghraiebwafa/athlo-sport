namespace Athlo.Models.DTOs.Achievements;

public class AchievementDto
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Color { get; set; } = "#007AFF";
    public bool Unlocked { get; set; }
    public DateTime? UnlockedAt { get; set; }
}
