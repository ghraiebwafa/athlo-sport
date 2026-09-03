using System.Text.Json;
using Athlo.Models.DTOs.Auth;

namespace Athlo.Models.Helpers;

/// <summary>Parser/serializer for <c>users.preferences_json</c>.</summary>
public static class UserPreferencesJson
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
            return Normalize(new UserPreferencesDto());

        try
        {
            var parsed = JsonSerializer.Deserialize<UserPreferencesDto>(json, JsonOptions);
            return Normalize(parsed ?? new UserPreferencesDto());
        }
        catch (JsonException)
        {
            return Normalize(new UserPreferencesDto());
        }
    }

    public static string Serialize(UserPreferencesDto preferences) =>
        JsonSerializer.Serialize(Normalize(preferences), JsonOptions);

    public static UserPreferencesDto Normalize(UserPreferencesDto preferences) => new()
    {
        NotifyWorkoutReminders = preferences.NotifyWorkoutReminders,
        NotifyPrAlerts = preferences.NotifyPrAlerts,
        NotifyStreakReminders = preferences.NotifyStreakReminders,
        PushPermissionAsked = preferences.PushPermissionAsked,
        HeartRateSource = string.Equals(preferences.HeartRateSource, "manual", StringComparison.OrdinalIgnoreCase)
            ? "manual"
            : "estimated",
        DefaultRestSeconds = RestPresets.Contains(preferences.DefaultRestSeconds)
            ? preferences.DefaultRestSeconds
            : Defaults.DefaultRestSeconds,
        BetweenExerciseRestSeconds = RestPresets.Contains(preferences.BetweenExerciseRestSeconds)
            ? preferences.BetweenExerciseRestSeconds
            : Defaults.BetweenExerciseRestSeconds
    };
}
