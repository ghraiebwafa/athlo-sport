using Athlo.ManagementService;
using Athlo.Shared.Configuration;
using Athlo.Shared.Extensions;

EnvConfiguration.LoadEnvFile();

var builder = WebApplication.CreateBuilder(args);
builder.AddAthloLogging("Athlo.ManagementService");
builder.AddAthloSentry("Athlo.ManagementService");

builder.Configuration.AddEnvironmentVariables();
if (!builder.Environment.IsEnvironment("Testing"))
    builder.Configuration.ValidateAthloConfiguration();

var startup = new Startup(builder.Configuration, builder.Environment);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

await startup.InitializeAsync(app);
startup.Configure(app);

app.Run();
