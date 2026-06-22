using Athlo.Database.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Athlo.Database;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AthloDbContext>
{
    public AthloDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var envPath = Path.Combine(basePath, "..", "..", ".env");
        if (File.Exists(envPath))
            DotNetEnv.Env.Load(envPath);

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found for migrations.");

        var optionsBuilder = new DbContextOptionsBuilder<AthloDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AthloDbContext(optionsBuilder.Options);
    }
}
