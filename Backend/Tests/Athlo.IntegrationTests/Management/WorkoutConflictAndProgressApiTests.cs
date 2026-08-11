using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Athlo.Database.DbContexts;
using Athlo.Models.DTOs.Programs;
using Athlo.Models.DTOs.Progress;
using Athlo.Models.DTOs.Workouts;
using Microsoft.Extensions.DependencyInjection;

namespace Athlo.IntegrationTests.Management;

[Collection("ManagementApi")]
public class WorkoutConflictAndProgressApiTests(ManagementWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task StartWorkout_WhenAlreadyActive_ReturnsConflict()
    {
        var user = TestJwtHelper.CreateTestUser($"conflict_{Guid.NewGuid():N}@test.local");
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AthloDbContext>();
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtHelper.CreateAccessToken(user));

        var programs = await _client.GetFromJsonAsync<List<ProgramListItemDto>>("/api/programs", TestJsonOptions.Default);
        Assert.NotNull(programs);
        Assert.NotEmpty(programs);

        var programId = programs[0].Id;
        var first = await _client.PostAsJsonAsync("/api/workouts/start", new StartWorkoutRequest { ProgramId = programId });
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await _client.PostAsJsonAsync("/api/workouts/start", new StartWorkoutRequest { ProgramId = programId });
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task CancelWorkout_ClearsActiveSession()
    {
        var user = TestJwtHelper.CreateTestUser($"cancel_{Guid.NewGuid():N}@test.local");
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AthloDbContext>();
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtHelper.CreateAccessToken(user));

        var programs = await _client.GetFromJsonAsync<List<ProgramListItemDto>>("/api/programs", TestJsonOptions.Default);
        var programId = programs![0].Id;

        var start = await _client.PostAsJsonAsync("/api/workouts/start", new StartWorkoutRequest { ProgramId = programId });
        var session = await start.Content.ReadFromJsonAsync<WorkoutSessionDto>(TestJsonOptions.Default);
        Assert.NotNull(session);

        var cancel = await _client.PostAsJsonAsync("/api/workouts/cancel", new CancelWorkoutRequest { SessionId = session.Id });
        Assert.Equal(HttpStatusCode.OK, cancel.StatusCode);

        var active = await _client.GetAsync("/api/workouts/active");
        Assert.Equal(HttpStatusCode.NoContent, active.StatusCode);
    }

    [Fact]
    public async Task GetProgress_ReturnsAggregatesAfterCompletedWorkout()
    {
        var user = TestJwtHelper.CreateTestUser($"progress_{Guid.NewGuid():N}@test.local");
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AthloDbContext>();
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtHelper.CreateAccessToken(user));

        var programs = await _client.GetFromJsonAsync<List<ProgramListItemDto>>("/api/programs", TestJsonOptions.Default);
        var programId = programs![0].Id;

        var start = await _client.PostAsJsonAsync("/api/workouts/start", new StartWorkoutRequest { ProgramId = programId });
        var session = await start.Content.ReadFromJsonAsync<WorkoutSessionDto>(TestJsonOptions.Default);

        var complete = await _client.PostAsJsonAsync("/api/workouts/complete", new CompleteWorkoutRequest
        {
            SessionId = session!.Id,
            CaloriesBurned = 180
        });
        Assert.Equal(HttpStatusCode.OK, complete.StatusCode);

        var progressResponse = await _client.GetAsync("/api/progress");
        Assert.Equal(HttpStatusCode.OK, progressResponse.StatusCode);

        var progress = await progressResponse.Content.ReadFromJsonAsync<ProgressResponse>(TestJsonOptions.Default);
        Assert.NotNull(progress);
        Assert.True(progress.TotalWorkouts >= 1);
        Assert.True(progress.TotalCaloriesBurned >= 180);
        Assert.NotEmpty(progress.RecentWorkouts);
    }
}
