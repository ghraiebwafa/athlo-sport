using Athlo.Database.DbContexts;
using Microsoft.Extensions.DependencyInjection;

namespace Athlo.IntegrationTests;

public static class InMemoryDbHelper
{
    public static void EnsureCreated(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AthloDbContext>();
        context.Database.EnsureCreated();
    }
}
