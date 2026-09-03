using Athlo.Models.DTOs.Auth;
using Athlo.Models.Helpers;

namespace Athlo.AuthService.Services;

/// <summary>Auth-layer alias for shared preferences JSON helpers.</summary>
public static class UserPreferencesHelper
{
    public static UserPreferencesDto Defaults => UserPreferencesJson.Defaults;

    public static UserPreferencesDto Parse(string? json) => UserPreferencesJson.Parse(json);

    public static string Serialize(UserPreferencesDto preferences) =>
        UserPreferencesJson.Serialize(preferences);

    public static UserPreferencesDto Normalize(UserPreferencesDto preferences) =>
        UserPreferencesJson.Normalize(preferences);
}
