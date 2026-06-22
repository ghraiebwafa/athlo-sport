using FluentValidation;

namespace Athlo.Shared.Validation;

public static class UrlValidationRules
{
    public static IRuleBuilderOptions<T, string?> MustBeHttpsUrl<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder
            .Must(BeHttpsUrl)
            .WithMessage("Image URL must be a valid HTTPS URL.");

    private static bool BeHttpsUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        return Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri)
               && uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
    }
}
