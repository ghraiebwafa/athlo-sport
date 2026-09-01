using System.Text.Json;
using Athlo.Models.DTOs.Auth;

namespace Athlo.AuthService.Services;

public static class UserPreferencesHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private static readonly int[] RestPresets = [60, 90, 120];

    public static UserPreferencesDto Defaults { get; } = new();

    public static UserPreferencesDto Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return CloneDefaults();

        try
        {
            var parsed = JsonSerializer.Deserialize<UserPreferencesDto>(json, JsonOptions);
            return Normalize(parsed ?? new UserPreferencesDto());
        }
        catch (JsonException)
        {
            return CloneDefaults();
        }
    }

    public static string Serialize(UserPreferencesDto preferences) =>
        JsonSerializer.Serialize(Normalize(preferences), JsonOptions);

    public static UserPreferencesDto Normalize(UserPreferencesDto preferences)
    {
        return new UserPreferencesDto
        {
            NotifyWorkoutReminders = preferences.NotifyWorkoutReminders,
            NotifyPrAlerts = preferences.NotifyPrAlerts,
            NotifyStreakReminders = preferences.NotifyStreakReminders,
            PushPermissionAsked = preferences.PushPermissionAsked,
            HeartRateSource = NormalizeHeartRateSource(preferences.HeartRateSource),
            DefaultRestSeconds = NormalizeRestSeconds(preferences.DefaultRestSeconds),
            BetweenExerciseRestSeconds = NormalizeRestSeconds(preferences.BetweenExerciseRestSeconds)
        };
    }

    private static UserPreferencesDto CloneDefaults() => Normalize(Defaults);

    private static string NormalizeHeartRateSource(string? value) =>
        string.Equals(value, "manual", StringComparison.OrdinalIgnoreCase) ? "manual" : "estimated";

    private static int NormalizeRestSeconds(int value) =>
        RestPresets.Contains(value) ? value : Defaults.DefaultRestSeconds;
}
