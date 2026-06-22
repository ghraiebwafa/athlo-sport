using Athlo.Database.DbContexts;
using Athlo.Database.Seed;
using Athlo.Shared.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Athlo.IntegrationTests.Auth;

public static class AuthTestSeed
{
    public static void EnsureSuperAdmin(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AthloDbContext>();
        var settings = scope.ServiceProvider.GetRequiredService<IOptions<SuperAdminSettings>>();
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AthloDbContext>>();

        SuperAdminSeeder.EnsureAsync(context, settings, logger).GetAwaiter().GetResult();
    }
}
