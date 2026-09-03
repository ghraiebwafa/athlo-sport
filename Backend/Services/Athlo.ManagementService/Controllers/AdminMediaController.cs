using Athlo.Models.DTOs.Media;
using Athlo.Shared.Authorization;
using Athlo.Shared.Exceptions;
using Athlo.Shared.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Athlo.ManagementService.Controllers;

/// <summary>
/// Admin media uploads for exercise and program imagery.
/// </summary>
[ApiController]
[Route("api/admin/media")]
[Authorize(Policy = AthloPolicies.AdminOrSuperAdmin)]
[EnableRateLimiting("api")]
public class AdminMediaController(
    IOptions<MediaSettings> mediaOptions,
    IWebHostEnvironment environment,
    ILogger<AdminMediaController> logger) : ControllerBase
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    };

    /// <summary>
    /// Uploads an image and returns a public URL suitable for ImageUrl fields.
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(6_000_000)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<MediaUploadResponse>> Upload(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            throw new AppException("No file was uploaded.", 400);

        var settings = mediaOptions.Value;
        if (file.Length > settings.MaxFileBytes)
            throw new AppException($"File exceeds the {settings.MaxFileBytes / (1024 * 1024)} MB limit.", 400);

        if (!AllowedContentTypes.Contains(file.ContentType))
            throw new AppException("Only JPEG, PNG, WebP, or GIF images are allowed.", 400);

        var extension = file.ContentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            _ => Path.GetExtension(file.FileName)
        };

        if (string.IsNullOrWhiteSpace(extension))
            extension = ".bin";

        var storageRoot = Path.IsPathRooted(settings.StoragePath)
            ? settings.StoragePath
            : Path.Combine(environment.ContentRootPath, settings.StoragePath);

        Directory.CreateDirectory(storageRoot);

        var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var fullPath = Path.Combine(storageRoot, fileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream, ct);
        }

        var baseUrl = settings.PublicBaseUrl.TrimEnd('/');
        var url = $"{baseUrl}/uploads/{fileName}";

        logger.LogInformation("Media uploaded File={FileName} Size={Size} Url={Url}", fileName, file.Length, url);

        return Ok(new MediaUploadResponse
        {
            Url = url,
            FileName = fileName,
            SizeBytes = file.Length,
            ContentType = file.ContentType
        });
    }
}
