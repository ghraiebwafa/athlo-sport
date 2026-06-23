using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Athlo.Models.DTOs.Auth;
using Athlo.Shared.Enums;

namespace Athlo.IntegrationTests.Auth;

[Collection("AuthApi")]
public class RefreshTokenApiTests(AuthWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task RefreshToken_RotatesAndRejectsOldToken()
    {
        var email = $"refresh_{Guid.NewGuid():N}@test.local";
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "Refresh User",
            Email = email,
            Password = "Password123",
            ConfirmPassword = "Password123",
            CurrentWeight = 70,
            GoalWeight = 65,
            FitnessGoal = FitnessGoal.LoseWeight
        });

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Default);
        Assert.NotNull(auth?.RefreshToken);

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest
        {
            RefreshToken = auth.RefreshToken
        });
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Default);
        Assert.NotNull(refreshed?.RefreshToken);
        Assert.NotEqual(auth.RefreshToken, refreshed.RefreshToken);

        var reuseResponse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest
        {
            RefreshToken = auth.RefreshToken
        });
        Assert.Equal(HttpStatusCode.Unauthorized, reuseResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken()
    {
        var email = $"logout_{Guid.NewGuid():N}@test.local";
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "Logout User",
            Email = email,
            Password = "Password123",
            ConfirmPassword = "Password123",
            CurrentWeight = 70,
            GoalWeight = 65,
            FitnessGoal = FitnessGoal.LoseWeight
        });

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Default);
        Assert.NotNull(auth);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var logoutResponse = await _client.PostAsJsonAsync("/api/auth/logout", new RefreshTokenRequest
        {
            RefreshToken = auth.RefreshToken
        });
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest
        {
            RefreshToken = auth.RefreshToken
        });
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }
}
