using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Athlo.Models.DTOs.Auth;
using Athlo.Shared.Enums;

namespace Athlo.IntegrationTests.Auth;

[Collection("AuthApi")]
public class AuthApiTests(AuthWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RegisterLoginAndGetProfile_Succeeds()
    {
        var email = $"user_{Guid.NewGuid():N}@test.local";

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "Test User",
            Email = email,
            Password = "Password123",
            ConfirmPassword = "Password123",
            CurrentWeight = 70,
            GoalWeight = 65,
            FitnessGoal = FitnessGoal.LoseWeight
        });

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Default);
        Assert.NotNull(auth?.AccessToken);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var profileResponse = await _client.GetAsync("/api/auth/profile");
        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);

        var profile = await profileResponse.Content.ReadFromJsonAsync<UserProfileResponse>(TestJsonOptions.Default);
        Assert.Equal(email, profile?.Email);
    }

    [Fact]
    public async Task ChangePassword_RevokesOldLogin()
    {
        var email = $"pwd_{Guid.NewGuid():N}@test.local";
        const string oldPassword = "Password123";
        const string newPassword = "NewPassword456";

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "Password User",
            Email = email,
            Password = oldPassword,
            ConfirmPassword = oldPassword,
            CurrentWeight = 70,
            GoalWeight = 65,
            FitnessGoal = FitnessGoal.LoseWeight
        });

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Default);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var changeResponse = await _client.PostAsJsonAsync("/api/auth/change-password", new ChangePasswordRequest
        {
            CurrentPassword = oldPassword,
            NewPassword = newPassword,
            ConfirmNewPassword = newPassword
        });

        Assert.Equal(HttpStatusCode.NoContent, changeResponse.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;

        var oldLogin = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = oldPassword
        });
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);

        var newLogin = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = newPassword
        });
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }
}
