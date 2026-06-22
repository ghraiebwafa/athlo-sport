using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Athlo.Models.DTOs.Auth;
using Athlo.Models.DTOs.Workouts;

namespace Athlo.IntegrationTests.Auth;

[Collection("AuthApi")]
public class AdminUsersApiTests(AuthWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetUsers_AsSuperAdmin_ReturnsOk()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = TestConfiguration.Values["SuperAdmin:Email"]!,
            Password = TestConfiguration.Values["SuperAdmin:Password"]!
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Default);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var usersResponse = await _client.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.OK, usersResponse.StatusCode);

        var users = await usersResponse.Content.ReadFromJsonAsync<PagedResult<Athlo.Models.DTOs.Admin.UserListItemDto>>(TestJsonOptions.Default);
        Assert.NotNull(users);
        Assert.NotEmpty(users.Items);
    }

    [Fact]
    public async Task GetUsers_WithoutAuth_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
