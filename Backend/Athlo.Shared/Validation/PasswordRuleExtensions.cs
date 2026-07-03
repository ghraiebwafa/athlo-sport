using FluentValidation;

namespace Athlo.Shared.Validation;

public static class PasswordRuleExtensions
{
    public static IRuleBuilderOptions<T, string> AthloUserPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule.MinimumLength(8)
            .MaximumLength(72).WithMessage("Password must not exceed 72 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a number.");

    public static IRuleBuilderOptions<T, string> AthloAdminPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule.MinimumLength(12)
            .MaximumLength(72).WithMessage("Password must not exceed 72 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a number.");
}
