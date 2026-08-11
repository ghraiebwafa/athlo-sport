using Athlo.Models.DTOs.Workouts;
using FluentValidation;

namespace Athlo.ManagementService.Validators;

public class UpdateSetRequestValidator : AbstractValidator<UpdateSetRequest>
{
    public UpdateSetRequestValidator()
    {
        RuleFor(x => x.RepsCompleted).InclusiveBetween(0, 500);
        RuleFor(x => x.WeightKg)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(1000)
            .When(x => x.WeightKg.HasValue);
    }
}
