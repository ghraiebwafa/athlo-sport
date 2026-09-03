using FluentValidation;

namespace Athlo.Shared.Validation;

public static class UrlValidationRules
{
    public static IRuleBuilderOptions<T, string?> MustBeHttpsUrl<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder
            .Must(BeAllowedImageUrl)
            .WithMessage("Image URL must be HTTPS, a local upload URL, or a relative /uploads/ path.");

    private static bool BeAllowedImageUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        var trimmed = value.Trim();
        if (trimmed.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            return trimmed.Length < 500;

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            return true;

        // Local development / docker host uploads over HTTP.
        if (uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            && (uri.Host is "localhost" or "127.0.0.1" || uri.Host.EndsWith(".local", StringComparison.OrdinalIgnoreCase)))
            return true;

        return false;
    }
}
