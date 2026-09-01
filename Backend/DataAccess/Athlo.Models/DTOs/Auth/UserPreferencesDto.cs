namespace Athlo.Models.DTOs.Auth;

public class UserPreferencesDto
{
    public bool NotifyWorkoutReminders { get; set; } = true;
    public bool NotifyPrAlerts { get; set; } = true;
    public bool NotifyStreakReminders { get; set; } = false;
    public bool PushPermissionAsked { get; set; } = false;
    public string HeartRateSource { get; set; } = "estimated";
    public int DefaultRestSeconds { get; set; } = 90;
    public int BetweenExerciseRestSeconds { get; set; } = 120;
}
