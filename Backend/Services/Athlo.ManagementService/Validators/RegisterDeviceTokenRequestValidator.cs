using Athlo.Models.DTOs.Notifications;
using FluentValidation;

namespace Athlo.ManagementService.Validators;

public class RegisterDeviceTokenRequestValidator : AbstractValidator<RegisterDeviceTokenRequest>
{
    public RegisterDeviceTokenRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .MaximumLength(512);

        RuleFor(x => x.Platform)
            .MaximumLength(32)
            .When(x => !string.IsNullOrWhiteSpace(x.Platform));
    }
}
