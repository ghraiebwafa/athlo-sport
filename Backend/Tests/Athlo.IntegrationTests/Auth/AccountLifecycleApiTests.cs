using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Athlo.Models.DTOs.Auth;
using Athlo.Shared.Enums;

namespace Athlo.IntegrationTests.Auth;

[Collection("AuthApi")]
public class AccountLifecycleApiTests(AuthWebApplicationFactory factory)
{
    [Fact]
    public async Task ExportAccount_ReturnsProfileAndCollections()
    {
        var email = $"export_{Guid.NewGuid():N}@test.local";
        const string password = "Password123";

        using var client = factory.CreateClient();
        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "Export User",
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

        var export = await client.GetFromJsonAsync<UserDataExportDto>(
            "/api/auth/account/export",
            TestJsonOptions.Default);

        Assert.NotNull(export);
        Assert.Equal(email, export.Profile.Email);
        Assert.NotNull(export.Preferences);
        Assert.Empty(export.Workouts);
    }

    [Fact]
    public async Task DeleteAccount_RequiresPassword_AndRevokesAccess()
    {
        var email = $"del_{Guid.NewGuid():N}@test.local";
        const string password = "Password123";

        using var client = factory.CreateClient();
        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "Delete User",
            Email = email,
            Password = password,
            ConfirmPassword = password,
            CurrentWeight = 70,
            GoalWeight = 65,
            FitnessGoal = FitnessGoal.LoseWeight
        });
        var auth = await register.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Default);
        var accessToken = auth!.AccessToken;
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var wrongPassword = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/auth/account")
        {
            Content = JsonContent.Create(new DeleteAccountRequest { Password = "WrongPassword1" })
        });
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);

        var deleted = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/auth/account")
        {
            Content = JsonContent.Create(new DeleteAccountRequest { Password = password })
        });
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var profileAfter = await client.GetAsync("/api/auth/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, profileAfter.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }
}
