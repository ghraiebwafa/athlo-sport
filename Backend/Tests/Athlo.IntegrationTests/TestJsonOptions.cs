using System.Text.Json;
using System.Text.Json.Serialization;

namespace Athlo.IntegrationTests;

public static class TestJsonOptions
{
    public static JsonSerializerOptions Default { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
}
