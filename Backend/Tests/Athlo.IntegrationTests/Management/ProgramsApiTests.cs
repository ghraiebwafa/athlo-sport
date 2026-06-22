using System.Net;
using System.Net.Http.Json;
using Athlo.Models.DTOs.Programs;

namespace Athlo.IntegrationTests.Management;

[Collection("ManagementApi")]
public class ProgramsApiTests(ManagementWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetPrograms_WithoutAuth_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/programs");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var programs = await response.Content.ReadFromJsonAsync<List<ProgramListItemDto>>(TestJsonOptions.Default);
        Assert.NotNull(programs);
        Assert.NotEmpty(programs);
    }

    [Fact]
    public async Task GetCategories_WithoutAuth_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/programs/categories");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>(TestJsonOptions.Default);
        Assert.NotNull(categories);
        Assert.NotEmpty(categories);
    }

    [Fact]
    public async Task GetWorkouts_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/workouts/history");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
