using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Athlo.Database.DbContexts;
using Athlo.Models.DTOs.Programs;
using Athlo.Models.DTOs.Workouts;
using Microsoft.Extensions.DependencyInjection;

namespace Athlo.IntegrationTests.Management;

[Collection("ManagementApi")]
public class WorkoutLifecycleApiTests(ManagementWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task WorkoutLifecycle_StartCompleteAndHistory()
    {
        var user = TestJwtHelper.CreateTestUser($"workout_{Guid.NewGuid():N}@test.local");
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AthloDbContext>();
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtHelper.CreateAccessToken(user));

        var programsResponse = await _client.GetAsync("/api/programs");
        Assert.Equal(HttpStatusCode.OK, programsResponse.StatusCode);
        var programs = await programsResponse.Content.ReadFromJsonAsync<List<ProgramListItemDto>>(TestJsonOptions.Default);
        Assert.NotNull(programs);
        Assert.NotEmpty(programs);

        var programId = programs[0].Id;
        var startResponse = await _client.PostAsJsonAsync("/api/workouts/start", new StartWorkoutRequest
        {
            ProgramId = programId
        });
        Assert.Equal(HttpStatusCode.OK, startResponse.StatusCode);

        var active = await startResponse.Content.ReadFromJsonAsync<WorkoutSessionDto>(TestJsonOptions.Default);
        Assert.NotNull(active);

        var completeResponse = await _client.PostAsJsonAsync("/api/workouts/complete", new CompleteWorkoutRequest
        {
            SessionId = active.Id,
            CaloriesBurned = 250
        });
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);

        var historyResponse = await _client.GetAsync("/api/workouts/history?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, historyResponse.StatusCode);
    }
}
