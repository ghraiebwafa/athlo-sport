using Athlo.Database.DbContexts;
using Athlo.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Athlo.Database;

public static class DependencyInjection
{
    public static IServiceCollection AddAthloDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        if (IsTestingEnvironment())
        {
            var databaseName = configuration["ATHLO_INMEMORY_DB"]
                ?? Environment.GetEnvironmentVariable("ATHLO_INMEMORY_DB")
                ?? "AthloTests";
            services.AddDbContext<AthloDbContext>(options => options.UseInMemoryDatabase(databaseName));
        }
        else
        {
            var connectionString = ConnectionStringBuilder.GetDefaultConnection(configuration);
            services.AddDbContext<AthloDbContext>(options =>
                options.UseNpgsql(connectionString, npgsql =>
                    npgsql.EnableRetryOnFailure(maxRetryCount: 3)));
        }

        services.AddHealthChecks()
            .AddDbContextCheck<AthloDbContext>(name: "database");

        return services;
    }

    private static bool IsTestingEnvironment() =>
        string.Equals(
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            "Testing",
            StringComparison.OrdinalIgnoreCase);
}
