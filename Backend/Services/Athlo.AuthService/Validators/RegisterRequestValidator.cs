using Athlo.Models.DTOs.Auth;
using Athlo.Shared.Validation;
using FluentValidation;

namespace Athlo.AuthService.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).AthloUserPassword();
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Passwords do not match.");
        RuleFor(x => x.CurrentWeight).InclusiveBetween(20, 500);
        RuleFor(x => x.GoalWeight).InclusiveBetween(20, 500);
    }
}
