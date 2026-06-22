using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Athlo.IntegrationTests.Auth;

public class AuthWebApplicationFactory : WebApplicationFactory<Athlo.AuthService.Program>
{
    private readonly string _databaseName = $"AthloAuthTest_{Guid.NewGuid()}";
    private int _initialized;

    public AuthWebApplicationFactory() => TestEnvironment.Apply();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var settings = new Dictionary<string, string?>(TestConfiguration.Values)
            {
                ["ATHLO_INMEMORY_DB"] = _databaseName
            };
            config.AddInMemoryCollection(settings);
        });
    }

    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);

        if (Interlocked.CompareExchange(ref _initialized, 1, 0) != 0)
            return;
        InMemoryDbHelper.EnsureCreated(Services);
        AuthTestSeed.EnsureSuperAdmin(Services);
    }
}
