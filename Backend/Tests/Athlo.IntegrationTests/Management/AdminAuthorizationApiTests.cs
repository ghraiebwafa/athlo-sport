using System.Net;
using System.Net.Http.Json;
using Athlo.Models.DTOs.Admin;
using Athlo.Models.DTOs.Exercises;
using Athlo.Models.DTOs.Programs;
using Athlo.Shared.Enums;

namespace Athlo.IntegrationTests.Management;

[Collection("ManagementApi")]
public class AdminAuthorizationApiTests(ManagementWebApplicationFactory factory)
{
    private static readonly Guid SeedCategoryId = Guid.Parse("11111111-1111-1111-1111-111111111101");
    private static readonly Guid SeedExerciseId = Guid.Parse("33333333-3333-3333-3333-333333333301");

    [Fact]
    public async Task AdminCatalogRoutes_AsUser_ReturnForbidden()
    {
        using var client = TestJwtHelper.CreateAuthorizedClient(factory, UserRole.User);

        var program = await client.PostAsJsonAsync("/api/admin/programs", ValidProgram());
        Assert.Equal(HttpStatusCode.Forbidden, program.StatusCode);

        var exercise = await client.PostAsJsonAsync("/api/admin/exercises", new CreateExerciseRequest
        {
            Name = $"Denied {Guid.NewGuid():N}"
        });
        Assert.Equal(HttpStatusCode.Forbidden, exercise.StatusCode);

        var category = await client.PostAsJsonAsync("/api/admin/categories", new CreateCategoryRequest
        {
            Name = "Denied",
            Slug = $"denied-{Guid.NewGuid():N}",
            Icon = "x"
        });
        Assert.Equal(HttpStatusCode.Forbidden, category.StatusCode);
    }

    [Fact]
    public async Task AdminStats_AsAdmin_ReturnsForbidden()
    {
        using var client = TestJwtHelper.CreateAuthorizedClient(factory, UserRole.Admin);
        var response = await client.GetAsync("/api/admin/stats");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminStats_AsSuperAdmin_ReturnsOk()
    {
        using var client = TestJwtHelper.CreateAuthorizedClient(factory, UserRole.SuperAdmin);
        var response = await client.GetAsync("/api/admin/stats");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var stats = await response.Content.ReadFromJsonAsync<AdminDashboardStatsDto>(TestJsonOptions.Default);
        Assert.NotNull(stats);
        Assert.True(stats.TotalPrograms >= 1);
        Assert.True(stats.TotalExercises >= 1);
    }

    [Fact]
    public async Task AdminCatalogRoutes_WithoutAuth_ReturnUnauthorized()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/admin/programs", ValidProgram());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var stats = await client.GetAsync("/api/admin/stats");
        Assert.Equal(HttpStatusCode.Unauthorized, stats.StatusCode);
    }

    private static CreateProgramRequest ValidProgram() => new()
    {
        Name = "Denied Program",
        Description = "Should not create",
        DurationMinutes = 30,
        Difficulty = WorkoutDifficulty.Beginner,
        EstimatedCalories = 200,
        CategoryId = SeedCategoryId,
        Exercises =
        [
            new ProgramExerciseInput
            {
                ExerciseId = SeedExerciseId,
                OrderIndex = 1,
                Sets = 3,
                Reps = 10
            }
        ]
    };
}
