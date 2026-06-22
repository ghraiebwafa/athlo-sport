using Microsoft.Extensions.Configuration;

namespace Athlo.Shared.Configuration;

public static class ConnectionStringBuilder
{
    public static string GetDefaultConnection(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(connectionString))
            return connectionString;

        var host = configuration["POSTGRES_HOST"] ?? "localhost";
        var port = configuration["POSTGRES_PORT"] ?? "5432";
        var database = configuration["POSTGRES_DB"] ?? "athlo";
        var username = configuration["POSTGRES_USER"] ?? "athlo";
        var password = configuration["POSTGRES_PASSWORD"]
            ?? throw new InvalidOperationException("POSTGRES_PASSWORD is required when ConnectionStrings__DefaultConnection is not set.");

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }
}
