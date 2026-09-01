using Athlo.Models.DTOs.Auth;
using FluentValidation;

namespace Athlo.AuthService.Validators;

public class UserPreferencesDtoValidator : AbstractValidator<UserPreferencesDto>
{
    public UserPreferencesDtoValidator()
    {
        RuleFor(x => x.HeartRateSource)
            .Must(v => v is "estimated" or "manual")
            .WithMessage("Heart rate source must be 'estimated' or 'manual'.");

        RuleFor(x => x.DefaultRestSeconds)
            .Must(v => v is 60 or 90 or 120)
            .WithMessage("Default rest must be 60, 90, or 120 seconds.");

        RuleFor(x => x.BetweenExerciseRestSeconds)
            .Must(v => v is 60 or 90 or 120)
            .WithMessage("Between-exercise rest must be 60, 90, or 120 seconds.");
    }
}
