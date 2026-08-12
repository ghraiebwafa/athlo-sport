using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Athlo.Database.DbContexts;
using Athlo.Models.DTOs.Workouts;
using Athlo.Models.Entities;
using Athlo.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Athlo.IntegrationTests.Management;

[Collection("ManagementApi")]
public class WorkoutOwnershipApiTests(ManagementWebApplicationFactory factory)
{
    [Fact]
    public async Task WorkoutMutations_ReturnNotFound_ForOtherUsersSession()
    {
        var owner = TestJwtHelper.CreateTestUser($"owner_{Guid.NewGuid():N}@test.local");
        var intruder = TestJwtHelper.CreateTestUser($"intruder_{Guid.NewGuid():N}@test.local");
        Guid sessionId;
        Guid programId;
        Guid programExerciseId;
        Guid setLogId;

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AthloDbContext>();
            context.Users.AddRange(owner, intruder);
            await context.SaveChangesAsync();

            var program = context.WorkoutPrograms.First();
            programId = program.Id;
            programExerciseId = context.ProgramExercises.First(pe => pe.ProgramId == programId).Id;

            var session = new WorkoutSession
            {
                Id = Guid.NewGuid(),
                UserId = owner.Id,
                ProgramId = programId,
                StartedAt = DateTime.UtcNow,
                Status = WorkoutSessionStatus.InProgress
            };
            context.WorkoutSessions.Add(session);

            var setLog = new WorkoutSetLog
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                ProgramExerciseId = programExerciseId,
                ExerciseId = context.ProgramExercises.First(pe => pe.Id == programExerciseId).ExerciseId,
                SetNumber = 1,
                RepsCompleted = 5,
                WeightKg = 40,
                Completed = true,
                LoggedAt = DateTime.UtcNow
            };
            context.WorkoutSetLogs.Add(setLog);
            await context.SaveChangesAsync();

            sessionId = session.Id;
            setLogId = setLog.Id;
        }

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtHelper.CreateAccessToken(intruder));

        var complete = await client.PostAsJsonAsync("/api/workouts/complete", new CompleteWorkoutRequest
        {
            SessionId = sessionId,
            CaloriesBurned = 100
        });
        Assert.Equal(HttpStatusCode.NotFound, complete.StatusCode);

        var cancel = await client.PostAsJsonAsync("/api/workouts/cancel", new CancelWorkoutRequest
        {
            SessionId = sessionId
        });
        Assert.Equal(HttpStatusCode.NotFound, cancel.StatusCode);

        var logSet = await client.PostAsJsonAsync($"/api/workouts/{sessionId}/sets", new LogSetRequest
        {
            ProgramExerciseId = programExerciseId,
            SetNumber = 2,
            RepsCompleted = 8,
            WeightKg = 45,
            Completed = true
        });
        Assert.Equal(HttpStatusCode.NotFound, logSet.StatusCode);

        var updateSet = await client.PutAsJsonAsync($"/api/workouts/sets/{setLogId}", new UpdateSetRequest
        {
            RepsCompleted = 10,
            WeightKg = 50,
            Completed = true
        });
        Assert.Equal(HttpStatusCode.NotFound, updateSet.StatusCode);

        // Owner still owns an active session (intruder could not mutate it).
        using var ownerClient = factory.CreateClient();
        ownerClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtHelper.CreateAccessToken(owner));
        var active = await ownerClient.GetFromJsonAsync<WorkoutSessionDto>("/api/workouts/active", TestJsonOptions.Default);
        Assert.NotNull(active);
        Assert.Equal(sessionId, active.Id);
    }
}
