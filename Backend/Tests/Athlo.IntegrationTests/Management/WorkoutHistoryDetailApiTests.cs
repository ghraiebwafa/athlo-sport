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
public class WorkoutHistoryDetailApiTests(ManagementWebApplicationFactory factory)
{
    [Fact]
    public async Task GetHistorySession_ReturnsSets_ForOwner()
    {
        var user = TestJwtHelper.CreateTestUser($"hist_{Guid.NewGuid():N}@test.local");
        Guid sessionId;

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AthloDbContext>();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var program = context.WorkoutPrograms.First();
            var programExercise = context.ProgramExercises.First(pe => pe.ProgramId == program.Id);

            var session = new WorkoutSession
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ProgramId = program.Id,
                StartedAt = DateTime.UtcNow.AddMinutes(-40),
                CompletedAt = DateTime.UtcNow.AddMinutes(-10),
                CaloriesBurned = 220,
                Status = WorkoutSessionStatus.Completed
            };
            context.WorkoutSessions.Add(session);
            context.WorkoutSetLogs.Add(new WorkoutSetLog
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                ProgramExerciseId = programExercise.Id,
                ExerciseId = programExercise.ExerciseId,
                SetNumber = 1,
                RepsCompleted = 10,
                WeightKg = 50,
                Completed = true,
                LoggedAt = DateTime.UtcNow.AddMinutes(-30)
            });
            await context.SaveChangesAsync();
            sessionId = session.Id;
        }

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtHelper.CreateAccessToken(user));

        var detail = await client.GetFromJsonAsync<WorkoutSessionDto>(
            $"/api/workouts/history/{sessionId}",
            TestJsonOptions.Default);

        Assert.NotNull(detail);
        Assert.Equal(sessionId, detail.Id);
        Assert.Single(detail.Sets);
        Assert.Equal(50, detail.Sets[0].WeightKg);
    }

    [Fact]
    public async Task GetHistorySession_ReturnsNotFound_ForOtherUser()
    {
        var owner = TestJwtHelper.CreateTestUser($"hist_owner_{Guid.NewGuid():N}@test.local");
        var intruder = TestJwtHelper.CreateTestUser($"hist_intr_{Guid.NewGuid():N}@test.local");
        Guid sessionId;

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AthloDbContext>();
            context.Users.AddRange(owner, intruder);
            await context.SaveChangesAsync();

            var program = context.WorkoutPrograms.First();
            var session = new WorkoutSession
            {
                Id = Guid.NewGuid(),
                UserId = owner.Id,
                ProgramId = program.Id,
                StartedAt = DateTime.UtcNow.AddHours(-1),
                CompletedAt = DateTime.UtcNow,
                CaloriesBurned = 100,
                Status = WorkoutSessionStatus.Completed
            };
            context.WorkoutSessions.Add(session);
            await context.SaveChangesAsync();
            sessionId = session.Id;
        }

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtHelper.CreateAccessToken(intruder));

        var response = await client.GetAsync($"/api/workouts/history/{sessionId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
