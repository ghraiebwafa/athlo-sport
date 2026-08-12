using System.Net;
using System.Net.Http.Json;
using Athlo.Models.DTOs.Exercises;
using Athlo.Models.DTOs.Programs;
using Athlo.Shared.Enums;

namespace Athlo.IntegrationTests.Management;

[Collection("ManagementApi")]
public class AdminCatalogCrudApiTests(ManagementWebApplicationFactory factory)
{
    private static readonly Guid SeedCategoryId = Guid.Parse("11111111-1111-1111-1111-111111111101");
    private static readonly Guid SeedExerciseId = Guid.Parse("33333333-3333-3333-3333-333333333301");

    [Fact]
    public async Task Admin_CanCreateUpdateAndDeleteProgram()
    {
        using var client = TestJwtHelper.CreateAuthorizedClient(factory, UserRole.Admin);
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var create = await client.PostAsJsonAsync("/api/admin/programs", new CreateProgramRequest
        {
            Name = $"Admin Program {suffix}",
            Description = "Created by admin integration test",
            DurationMinutes = 45,
            Difficulty = WorkoutDifficulty.Intermediate,
            EstimatedCalories = 350,
            IsFeatured = false,
            CategoryId = SeedCategoryId,
            Exercises =
            [
                new ProgramExerciseInput
                {
                    ExerciseId = SeedExerciseId,
                    OrderIndex = 1,
                    Sets = 4,
                    Reps = 8
                }
            ]
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var created = await create.Content.ReadFromJsonAsync<ProgramDetailDto>(TestJsonOptions.Default);
        Assert.NotNull(created);
        Assert.Equal($"Admin Program {suffix}", created.Name);
        Assert.Single(created.Exercises);

        var update = await client.PutAsJsonAsync($"/api/admin/programs/{created.Id}", new UpdateProgramRequest
        {
            Name = $"Updated Program {suffix}",
            Description = "Updated",
            DurationMinutes = 50,
            Difficulty = WorkoutDifficulty.Advanced,
            EstimatedCalories = 400,
            IsFeatured = true,
            CategoryId = SeedCategoryId,
            Exercises =
            [
                new ProgramExerciseInput
                {
                    ExerciseId = SeedExerciseId,
                    OrderIndex = 1,
                    Sets = 5,
                    Reps = 6
                }
            ]
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var updated = await update.Content.ReadFromJsonAsync<ProgramDetailDto>(TestJsonOptions.Default);
        Assert.NotNull(updated);
        Assert.Equal($"Updated Program {suffix}", updated.Name);
        Assert.True(updated.IsFeatured);
        Assert.Equal(5, updated.Exercises[0].Sets);

        var delete = await client.DeleteAsync($"/api/admin/programs/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var publicGet = await client.GetAsync($"/api/programs/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, publicGet.StatusCode);
    }

    [Fact]
    public async Task Admin_CanCreateUpdateAndDeleteExercise()
    {
        using var client = TestJwtHelper.CreateAuthorizedClient(factory, UserRole.Admin);
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var create = await client.PostAsJsonAsync("/api/admin/exercises", new CreateExerciseRequest
        {
            Name = $"Exercise {suffix}"
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var created = await create.Content.ReadFromJsonAsync<ExerciseDto>(TestJsonOptions.Default);
        Assert.NotNull(created);

        var update = await client.PutAsJsonAsync($"/api/admin/exercises/{created.Id}", new UpdateExerciseRequest
        {
            Name = $"Exercise Updated {suffix}"
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var updated = await update.Content.ReadFromJsonAsync<ExerciseDto>(TestJsonOptions.Default);
        Assert.NotNull(updated);
        Assert.Equal($"Exercise Updated {suffix}", updated.Name);

        var delete = await client.DeleteAsync($"/api/admin/exercises/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }

    [Fact]
    public async Task SuperAdmin_CanCreateUpdateAndDeleteCategory()
    {
        using var client = TestJwtHelper.CreateAuthorizedClient(factory, UserRole.SuperAdmin);
        var suffix = Guid.NewGuid().ToString("N")[..8].ToLowerInvariant();

        var create = await client.PostAsJsonAsync("/api/admin/categories", new CreateCategoryRequest
        {
            Name = $"Cat {suffix}",
            Slug = $"cat-{suffix}",
            Icon = "star"
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var created = await create.Content.ReadFromJsonAsync<CategoryDto>(TestJsonOptions.Default);
        Assert.NotNull(created);
        Assert.Equal($"cat-{suffix}", created.Slug);

        var update = await client.PutAsJsonAsync($"/api/admin/categories/{created.Id}", new UpdateCategoryRequest
        {
            Name = $"Cat Updated {suffix}",
            Slug = $"cat-{suffix}",
            Icon = "flame"
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var delete = await client.DeleteAsync($"/api/admin/categories/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }
}
