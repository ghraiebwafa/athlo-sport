using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Athlo.Models.DTOs.Auth;
using Athlo.Shared.Enums;

namespace Athlo.IntegrationTests.Auth;

[Collection("AuthApi")]
public class UserPreferencesApiTests(AuthWebApplicationFactory factory)
{
    [Fact]
    public async Task Preferences_GetUpdateAndPersist()
    {
        var email = $"prefs_{Guid.NewGuid():N}@test.local";
        const string password = "Password123";

        using var client = factory.CreateClient();

        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "Prefs User",
            Email = email,
            Password = password,
            ConfirmPassword = password,
            CurrentWeight = 70,
            GoalWeight = 65,
            FitnessGoal = FitnessGoal.LoseWeight
        });
        var auth = await register.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Default);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var defaults = await client.GetFromJsonAsync<UserPreferencesDto>(
            "/api/auth/preferences",
            TestJsonOptions.Default);
        Assert.NotNull(defaults);
        Assert.Equal(90, defaults.DefaultRestSeconds);
        Assert.Equal("estimated", defaults.HeartRateSource);

        var update = new UserPreferencesDto
        {
            NotifyWorkoutReminders = false,
            NotifyPrAlerts = true,
            NotifyStreakReminders = true,
            PushPermissionAsked = true,
            HeartRateSource = "manual",
            DefaultRestSeconds = 60,
            BetweenExerciseRestSeconds = 120
        };

        var putResponse = await client.PutAsJsonAsync("/api/auth/preferences", update);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var saved = await putResponse.Content.ReadFromJsonAsync<UserPreferencesDto>(TestJsonOptions.Default);
        Assert.NotNull(saved);
        Assert.False(saved.NotifyWorkoutReminders);
        Assert.Equal("manual", saved.HeartRateSource);
        Assert.Equal(60, saved.DefaultRestSeconds);

        var fetched = await client.GetFromJsonAsync<UserPreferencesDto>(
            "/api/auth/preferences",
            TestJsonOptions.Default);
        Assert.Equal(60, fetched!.DefaultRestSeconds);
        Assert.True(fetched.NotifyStreakReminders);
    }

    [Fact]
    public async Task Preferences_InvalidRestSeconds_ReturnsBadRequest()
    {
        var email = $"prefs_bad_{Guid.NewGuid():N}@test.local";
        const string password = "Password123";

        using var client = factory.CreateClient();
        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "Prefs User",
            Email = email,
            Password = password,
            ConfirmPassword = password,
            CurrentWeight = 70,
            GoalWeight = 65,
            FitnessGoal = FitnessGoal.LoseWeight
        });
        var auth = await register.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Default);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var response = await client.PutAsJsonAsync("/api/auth/preferences", new UserPreferencesDto
        {
            DefaultRestSeconds = 45
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
