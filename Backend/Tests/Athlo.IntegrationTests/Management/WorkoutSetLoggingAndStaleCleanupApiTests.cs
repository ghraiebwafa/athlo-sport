using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Athlo.Database.DbContexts;
using Athlo.ManagementService.Services;
using Athlo.Models.DTOs.Programs;
using Athlo.Models.DTOs.Progress;
using Athlo.Models.DTOs.Workouts;
using Athlo.Models.Entities;
using Athlo.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Athlo.IntegrationTests.Management;

[Collection("ManagementApi")]
public class WorkoutSetLoggingAndStaleCleanupApiTests(ManagementWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task LogSet_UpdatesActiveSessionAndPersonalRecords()
    {
        var user = TestJwtHelper.CreateTestUser($"sets_{Guid.NewGuid():N}@test.local");
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

        var detail = await _client.GetFromJsonAsync<ProgramDetailDto>($"/api/programs/{programId}", TestJsonOptions.Default);
        Assert.NotNull(detail);
        Assert.NotEmpty(detail.Exercises);

        var programExercise = detail.Exercises[0];

        var start = await _client.PostAsJsonAsync("/api/workouts/start", new StartWorkoutRequest { ProgramId = programId });
        var session = await start.Content.ReadFromJsonAsync<WorkoutSessionDto>(TestJsonOptions.Default);
        Assert.NotNull(session);

        var logResponse = await _client.PostAsJsonAsync($"/api/workouts/{session.Id}/sets", new LogSetRequest
        {
            ProgramExerciseId = programExercise.Id,
            SetNumber = 1,
            RepsCompleted = 8,
            WeightKg = 60,
            Completed = true
        });
        Assert.Equal(HttpStatusCode.OK, logResponse.StatusCode);

        var setLog = await logResponse.Content.ReadFromJsonAsync<WorkoutSetLogDto>(TestJsonOptions.Default);
        Assert.NotNull(setLog);
        Assert.Equal(60, setLog.WeightKg);
        Assert.Equal(8, setLog.RepsCompleted);

        var active = await _client.GetFromJsonAsync<WorkoutSessionDto>("/api/workouts/active", TestJsonOptions.Default);
        Assert.NotNull(active);
        Assert.Contains(active.Sets, s => s.Id == setLog.Id);

        var complete = await _client.PostAsJsonAsync("/api/workouts/complete", new CompleteWorkoutRequest
        {
            SessionId = session.Id,
            CaloriesBurned = 120
        });
        Assert.Equal(HttpStatusCode.OK, complete.StatusCode);

        var progress = await _client.GetFromJsonAsync<ProgressResponse>("/api/progress", TestJsonOptions.Default);
        Assert.NotNull(progress);
        Assert.True(progress.PersonalBests >= 1);
        Assert.Contains(progress.PersonalRecords, r => r.WeightKg == 60 && r.Reps == 8);
    }

    [Fact]
    public async Task CancelStaleSessions_AllowsStartingNewWorkout()
    {
        var user = TestJwtHelper.CreateTestUser($"stale_{Guid.NewGuid():N}@test.local");
        Guid programId;
        Guid staleSessionId;

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AthloDbContext>();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var program = context.WorkoutPrograms.First();
            programId = program.Id;

            var stale = new WorkoutSession
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ProgramId = programId,
                StartedAt = DateTime.UtcNow.AddHours(-30),
                Status = WorkoutSessionStatus.InProgress
            };
            context.WorkoutSessions.Add(stale);
            await context.SaveChangesAsync();
            staleSessionId = stale.Id;
        }

        using (var scope = factory.Services.CreateScope())
        {
            var workoutService = scope.ServiceProvider.GetRequiredService<IWorkoutService>();
            var cancelled = await workoutService.CancelStaleSessionsAsync(TimeSpan.FromHours(24));
            Assert.True(cancelled >= 1);
        }

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtHelper.CreateAccessToken(user));

        var start = await _client.PostAsJsonAsync("/api/workouts/start", new StartWorkoutRequest { ProgramId = programId });
        Assert.Equal(HttpStatusCode.OK, start.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AthloDbContext>();
            var stale = await context.WorkoutSessions.FindAsync(staleSessionId);
            Assert.NotNull(stale);
            Assert.Equal(WorkoutSessionStatus.Cancelled, stale.Status);
        }
    }
}
