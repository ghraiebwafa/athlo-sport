using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Athlo.Database.DbContexts;
using Athlo.Models.DTOs.Programs;
using Athlo.Models.DTOs.Workouts;
using Microsoft.Extensions.DependencyInjection;

namespace Athlo.IntegrationTests.Management;

[Collection("ManagementApi")]
public class WorkoutPauseResumeApiTests(ManagementWebApplicationFactory factory)
{
    [Fact]
    public async Task PauseAndResume_PersistsAcrossActiveFetch_AndAffectsDuration()
    {
        var user = TestJwtHelper.CreateTestUser($"pause_{Guid.NewGuid():N}@test.local");
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

        var start = await client.PostAsJsonAsync("/api/workouts/start", new StartWorkoutRequest { ProgramId = programId });
        var session = await start.Content.ReadFromJsonAsync<WorkoutSessionDto>(TestJsonOptions.Default);
        Assert.NotNull(session);
        Assert.False(session.IsPaused);

        var pause = await client.PostAsync($"/api/workouts/{session.Id}/pause", null);
        Assert.Equal(HttpStatusCode.OK, pause.StatusCode);
        var paused = await pause.Content.ReadFromJsonAsync<WorkoutSessionDto>(TestJsonOptions.Default);
        Assert.NotNull(paused);
        Assert.True(paused.IsPaused);
        Assert.NotNull(paused.PausedAt);

        var activeWhilePaused = await client.GetFromJsonAsync<WorkoutSessionDto>("/api/workouts/active", TestJsonOptions.Default);
        Assert.NotNull(activeWhilePaused);
        Assert.True(activeWhilePaused.IsPaused);

        await Task.Delay(1100);

        var resume = await client.PostAsync($"/api/workouts/{session.Id}/resume", null);
        Assert.Equal(HttpStatusCode.OK, resume.StatusCode);
        var resumed = await resume.Content.ReadFromJsonAsync<WorkoutSessionDto>(TestJsonOptions.Default);
        Assert.NotNull(resumed);
        Assert.False(resumed.IsPaused);
        Assert.Null(resumed.PausedAt);
        Assert.True(resumed.PausedDurationSeconds >= 1);

        var doublePause = await client.PostAsync($"/api/workouts/{session.Id}/pause", null);
        Assert.Equal(HttpStatusCode.OK, doublePause.StatusCode);

        var complete = await client.PostAsJsonAsync("/api/workouts/complete", new CompleteWorkoutRequest
        {
            SessionId = session.Id,
            CaloriesBurned = 100
        });
        Assert.Equal(HttpStatusCode.OK, complete.StatusCode);
        var completed = await complete.Content.ReadFromJsonAsync<WorkoutSessionDto>(TestJsonOptions.Default);
        Assert.NotNull(completed);
        Assert.False(completed.IsPaused);
        Assert.Null(completed.PausedAt);
        Assert.True(completed.PausedDurationSeconds >= 1);
        Assert.NotNull(completed.DurationMinutes);
    }

    [Fact]
    public async Task Pause_AsOtherUser_ReturnsNotFound()
    {
        var owner = TestJwtHelper.CreateTestUser($"pause_owner_{Guid.NewGuid():N}@test.local");
        var intruder = TestJwtHelper.CreateTestUser($"pause_intruder_{Guid.NewGuid():N}@test.local");
        Guid sessionId;

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AthloDbContext>();
            context.Users.AddRange(owner, intruder);
            await context.SaveChangesAsync();
        }

        using var ownerClient = factory.CreateClient();
        ownerClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtHelper.CreateAccessToken(owner));

        var programs = await ownerClient.GetFromJsonAsync<List<ProgramListItemDto>>("/api/programs", TestJsonOptions.Default);
        var start = await ownerClient.PostAsJsonAsync("/api/workouts/start", new StartWorkoutRequest
        {
            ProgramId = programs![0].Id
        });
        var session = await start.Content.ReadFromJsonAsync<WorkoutSessionDto>(TestJsonOptions.Default);
        sessionId = session!.Id;

        using var intruderClient = factory.CreateClient();
        intruderClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtHelper.CreateAccessToken(intruder));

        var pause = await intruderClient.PostAsync($"/api/workouts/{sessionId}/pause", null);
        Assert.Equal(HttpStatusCode.NotFound, pause.StatusCode);
    }
}
