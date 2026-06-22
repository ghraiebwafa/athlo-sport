namespace Athlo.IntegrationTests;

public static class TestEnvironment
{
    private static int _applied;

    public static void Apply()
    {
        if (Interlocked.Exchange(ref _applied, 1) == 1)
            return;

        foreach (var (key, value) in TestConfiguration.Values)
        {
            if (value is not null)
                Environment.SetEnvironmentVariable(key.Replace(":", "__"), value);
        }

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
    }
}
