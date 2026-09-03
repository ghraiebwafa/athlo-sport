using Athlo.Shared.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Athlo.Shared.Extensions;

/// <summary>Serves uploaded media from the configured storage folder at /uploads.</summary>
public static class MediaStaticFileExtensions
{
    public static WebApplication UseAthloMediaStaticFiles(
        this WebApplication app,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var settings = configuration.GetSection(MediaSettings.SectionName).Get<MediaSettings>()
            ?? new MediaSettings();

        var storageRoot = Path.IsPathRooted(settings.StoragePath)
            ? settings.StoragePath
            : Path.Combine(environment.ContentRootPath, settings.StoragePath);

        Directory.CreateDirectory(storageRoot);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(storageRoot),
            RequestPath = "/uploads",
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.CacheControl = "public,max-age=604800";
            }
        });

        return app;
    }
}
