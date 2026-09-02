using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Athlo.Models.DTOs.Admin;
using Athlo.Models.DTOs.Auth;
using Athlo.Shared.Enums;

namespace Athlo.IntegrationTests.Auth;

[Collection("AuthApi")]
public class AdminAdminsApiTests(AuthWebApplicationFactory factory)
{
    [Fact]
    public async Task SuperAdmin_CanCreateListAndRemoveAdmin()
    {
        using var client = factory.CreateClient();
        await AuthorizeAsSuperAdminAsync(client);

        var email = $"newadmin_{Guid.NewGuid():N}@test.local";
        const string password = "AdminPass123!";

        var create = await client.PostAsJsonAsync("/api/admin/admins", new CreateAdminRequest
        {
            FullName = "New Admin",
            Email = email,
            Password = password,
            ConfirmPassword = password
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var created = await create.Content.ReadFromJsonAsync<AdminUserDto>(TestJsonOptions.Default);
        Assert.NotNull(created);
        Assert.Equal(email, created.Email);
        Assert.Equal(UserRole.Admin, created.Role);

        var list = await client.GetAsync("/api/admin/admins");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var admins = await list.Content.ReadFromJsonAsync<List<AdminUserDto>>(TestJsonOptions.Default);
        Assert.NotNull(admins);
        Assert.Contains(admins, a => a.Id == created.Id);

        var loginAsAdmin = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });
        Assert.Equal(HttpStatusCode.OK, loginAsAdmin.StatusCode);

        var auth = await loginAsAdmin.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Default);
        var adminAccessToken = auth!.AccessToken;

        var remove = await client.DeleteAsync($"/api/admin/admins/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, remove.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var profileAfterDemote = await client.GetAsync("/api/auth/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, profileAfterDemote.StatusCode);

        await AuthorizeAsSuperAdminAsync(client);
        var listAfter = await client.GetFromJsonAsync<List<AdminUserDto>>("/api/admin/admins", TestJsonOptions.Default);
        Assert.DoesNotContain(listAfter!, a => a.Id == created.Id);
    }

    [Fact]
    public async Task Admin_CannotManageAdmins()
    {
        using var client = TestJwtHelper.CreateAuthorizedClient(factory, UserRole.Admin);

        var list = await client.GetAsync("/api/admin/admins");
        Assert.Equal(HttpStatusCode.Forbidden, list.StatusCode);

        var create = await client.PostAsJsonAsync("/api/admin/admins", new CreateAdminRequest
        {
            FullName = "Nope",
            Email = $"nope_{Guid.NewGuid():N}@test.local",
            Password = "AdminPass123!",
            ConfirmPassword = "AdminPass123!"
        });
        Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);
    }

    [Fact]
    public async Task User_CannotAccessAdminUserList()
    {
        using var client = TestJwtHelper.CreateAuthorizedClient(factory, UserRole.User);
        var response = await client.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CanListUsers()
    {
        using var client = TestJwtHelper.CreateAuthorizedClient(factory, UserRole.Admin);
        var response = await client.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SuperAdmin_CannotRemoveSelf()
    {
        using var client = factory.CreateClient();
        await AuthorizeAsSuperAdminAsync(client);

        var admins = await client.GetFromJsonAsync<List<AdminUserDto>>("/api/admin/admins", TestJsonOptions.Default);
        var superAdmin = Assert.Single(admins!, a => a.Role == UserRole.SuperAdmin);

        var remove = await client.DeleteAsync($"/api/admin/admins/{superAdmin.Id}");
        Assert.Equal(HttpStatusCode.Forbidden, remove.StatusCode);
    }

    private static async Task AuthorizeAsSuperAdminAsync(HttpClient client)
    {
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = TestConfiguration.Values["SuperAdmin:Email"]!,
            Password = TestConfiguration.Values["SuperAdmin:Password"]!
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>(TestJsonOptions.Default);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
    }
}
