using Athlo.Models.DTOs.Auth;
using FluentValidation;

namespace Athlo.AuthService.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(100)
            .When(x => x.FullName is not null);

        RuleFor(x => x.CurrentWeight)
            .InclusiveBetween(20, 500)
            .When(x => x.CurrentWeight.HasValue);

        RuleFor(x => x.GoalWeight)
            .InclusiveBetween(20, 500)
            .When(x => x.GoalWeight.HasValue);

        RuleFor(x => x.FitnessGoal)
            .IsInEnum()
            .When(x => x.FitnessGoal.HasValue);
    }
}
