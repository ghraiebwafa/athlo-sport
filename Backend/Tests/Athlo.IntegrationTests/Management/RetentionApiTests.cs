using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Athlo.Database.DbContexts;
using Athlo.Models.DTOs.Achievements;
using Athlo.Models.DTOs.Notifications;
using Athlo.Models.DTOs.Progress;
using Microsoft.Extensions.DependencyInjection;

namespace Athlo.IntegrationTests.Management;

[Collection("ManagementApi")]
public class RetentionApiTests(ManagementWebApplicationFactory factory)
{
    [Fact]
    public async Task PushToken_RegisterAndUnregister()
    {
        var user = TestJwtHelper.CreateTestUser($"push_{Guid.NewGuid():N}@test.local");
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AthloDbContext>();
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtHelper.CreateAccessToken(user));

        var body = new RegisterDeviceTokenRequest
        {
            Token = $"ExponentPushToken[{Guid.NewGuid():N}]",
            Platform = "ios"
        };

        var register = await client.PostAsJsonAsync("/api/devices/push-token", body, TestJsonOptions.Default);
        Assert.Equal(HttpStatusCode.NoContent, register.StatusCode);

        var unregister = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/devices/push-token")
        {
            Content = JsonContent.Create(body, options: TestJsonOptions.Default)
        });
        Assert.Equal(HttpStatusCode.NoContent, unregister.StatusCode);
    }

    [Fact]
    public async Task Achievements_ReturnsCatalog()
    {
        var user = TestJwtHelper.CreateTestUser($"ach_{Guid.NewGuid():N}@test.local");
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AthloDbContext>();
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtHelper.CreateAccessToken(user));

        var items = await client.GetFromJsonAsync<List<AchievementDto>>("/api/achievements", TestJsonOptions.Default);
        Assert.NotNull(items);
        Assert.Contains(items, a => a.Key == "first");
        Assert.All(items, a => Assert.False(a.Unlocked));
    }

    [Fact]
    public async Task WeeklySummary_ReturnsCurrentWeek()
    {
        var user = TestJwtHelper.CreateTestUser($"week_{Guid.NewGuid():N}@test.local");
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AthloDbContext>();
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtHelper.CreateAccessToken(user));

        var summary = await client.GetFromJsonAsync<WeeklySummaryDto>(
            "/api/progress/weekly-summary",
            TestJsonOptions.Default);
        Assert.NotNull(summary);
        Assert.Equal(0, summary.WorkoutsCompleted);
        Assert.False(string.IsNullOrWhiteSpace(summary.Headline));
    }

    [Fact]
    public async Task RetentionEndpoints_RequireAuth()
    {
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/achievements")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/progress/weekly-summary")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/devices/push-token", new { token = "x", platform = "ios" })).StatusCode);
    }
}
