using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Athlo.Database.DbContexts;
using Athlo.Models.DTOs.Programs;
using Athlo.Models.DTOs.Workouts;
using Athlo.Models.Entities;
using Athlo.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Athlo.IntegrationTests.Management;

[Collection("ManagementApi")]
public class SavedProgramsApiTests(ManagementWebApplicationFactory factory)
{
    [Fact]
    public async Task SavedPrograms_SaveListAndRemove()
    {
        var user = TestJwtHelper.CreateTestUser($"saved_{Guid.NewGuid():N}@test.local");
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AthloDbContext>();
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtHelper.CreateAccessToken(user));

        var programs = await client.GetFromJsonAsync<List<ProgramListItemDto>>("/api/programs", TestJsonOptions.Default);
        var programId = programs![0].Id;

        var empty = await client.GetFromJsonAsync<List<ProgramListItemDto>>("/api/programs/saved", TestJsonOptions.Default);
        Assert.Empty(empty!);

        var save = await client.PostAsync($"/api/programs/saved/{programId}", null);
        Assert.Equal(HttpStatusCode.NoContent, save.StatusCode);

        var status = await client.GetFromJsonAsync<SavedProgramStatusDto>(
            $"/api/programs/saved/{programId}",
            TestJsonOptions.Default);
        Assert.True(status!.Saved);

        var saved = await client.GetFromJsonAsync<List<ProgramListItemDto>>("/api/programs/saved", TestJsonOptions.Default);
        Assert.Contains(saved!, p => p.Id == programId);

        var remove = await client.DeleteAsync($"/api/programs/saved/{programId}");
        Assert.Equal(HttpStatusCode.NoContent, remove.StatusCode);
    }

    [Fact]
    public async Task SavedPrograms_RequiresAuth()
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/programs/saved");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

public class SavedProgramStatusDto
{
    public Guid ProgramId { get; set; }
    public bool Saved { get; set; }
}
