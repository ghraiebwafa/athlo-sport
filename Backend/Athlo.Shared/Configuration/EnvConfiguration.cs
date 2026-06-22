namespace Athlo.Shared.Configuration;

public static class EnvConfiguration
{
    private static readonly string[] RequiredVariables =
    [
        "Jwt__Secret",
        "SuperAdmin__Email",
        "SuperAdmin__Password"
    ];

    public static void LoadEnvFile()
    {
        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), ".env"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", ".env"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".env")
        };

        foreach (var candidate in candidates)
        {
            var path = Path.GetFullPath(candidate);
            if (!File.Exists(path))
                continue;

            DotNetEnv.Env.Load(path);
            return;
        }
    }

    public static void ValidateRequiredSettings(IConfiguration configuration)
    {
        var missing = RequiredVariables
            .Where(key => string.IsNullOrWhiteSpace(configuration[key.Replace("__", ":")]))
            .ToList();

        var hasConnectionString = !string.IsNullOrWhiteSpace(configuration.GetConnectionString("DefaultConnection"));
        var hasPostgresPassword = !string.IsNullOrWhiteSpace(configuration["POSTGRES_PASSWORD"]);

        if (!hasConnectionString && !hasPostgresPassword)
            missing.Add("ConnectionStrings__DefaultConnection or POSTGRES_PASSWORD");

        if (missing.Count == 0)
            return;

        throw new InvalidOperationException(
            "Missing required environment configuration: " +
            string.Join(", ", missing) +
            ". Copy .env.example to .env at the repository root and set your values.");
    }
}
