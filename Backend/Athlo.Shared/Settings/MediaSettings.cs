namespace Athlo.Shared.Settings;

public class MediaSettings
{
    public const string SectionName = "Media";

    /// <summary>Absolute filesystem directory for uploaded files.</summary>
    public string StoragePath { get; set; } = "App_Data/uploads";

    /// <summary>
    /// Public base URL used when returning image links (e.g. https://api.example.com or http://localhost:5000).
    /// Trailing slash is optional.
    /// </summary>
    public string PublicBaseUrl { get; set; } = "http://localhost:5000";

    /// <summary>Maximum upload size in bytes (default 5 MB).</summary>
    public long MaxFileBytes { get; set; } = 5 * 1024 * 1024;
}
