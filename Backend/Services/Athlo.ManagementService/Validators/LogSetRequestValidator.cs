using Athlo.Models.DTOs.Workouts;
using FluentValidation;

namespace Athlo.ManagementService.Validators;

public class LogSetRequestValidator : AbstractValidator<LogSetRequest>
{
    public LogSetRequestValidator()
    {
        RuleFor(x => x.ProgramExerciseId).NotEmpty();
        RuleFor(x => x.SetNumber).InclusiveBetween(1, 50);
        RuleFor(x => x.RepsCompleted).InclusiveBetween(0, 500);
        RuleFor(x => x.WeightKg)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(1000)
            .When(x => x.WeightKg.HasValue);
    }
}
