using System.Net;
using System.Net.Http.Json;
using Athlo.Models.DTOs.Exercises;

namespace Athlo.IntegrationTests.Management;

[Collection("ManagementApi")]
public class ExercisesApiTests(ManagementWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetExercises_WithoutAuth_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/exercises");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var exercises = await response.Content.ReadFromJsonAsync<List<ExerciseDto>>(TestJsonOptions.Default);
        Assert.NotNull(exercises);
        Assert.NotEmpty(exercises);
    }
}
