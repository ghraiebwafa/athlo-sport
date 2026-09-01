using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Athlo.Models.DTOs.Auth;
using Athlo.Shared.Enums;

namespace Athlo.IntegrationTests.Auth;

[Collection("AuthApi")]
public class LogoutAccessTokenRevocationApiTests(AuthWebApplicationFactory factory)
{
    [Fact]
    public async Task Logout_RevokesAccessTokenForProtectedRoutes()
    {
        var email = $"logout_{Guid.NewGuid():N}@test.local";
        const string password = "Password123";

        using var client = factory.CreateClient();

        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "Logout User",
            Email = email,
            Password = password,
            ConfirmPassword = password,
            CurrentWeight = 70,
            GoalWeight = 65,
            FitnessGoal = FitnessGoal.LoseWeight
        });
        Assert.Equal(HttpStatusCode.OK, register.StatusCode);

        var auth = await register.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Default);
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var profileBefore = await client.GetAsync("/api/auth/profile");
        Assert.Equal(HttpStatusCode.OK, profileBefore.StatusCode);

        var logout = await client.PostAsJsonAsync("/api/auth/logout", new RefreshTokenRequest
        {
            RefreshToken = auth.RefreshToken
        });
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var profileAfter = await client.GetAsync("/api/auth/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, profileAfter.StatusCode);
    }
}
