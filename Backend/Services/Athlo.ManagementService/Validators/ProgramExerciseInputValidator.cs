using Athlo.Models.DTOs.Programs;
using FluentValidation;

namespace Athlo.ManagementService.Validators;

public class ProgramExerciseInputValidator : AbstractValidator<ProgramExerciseInput>
{
    public ProgramExerciseInputValidator()
    {
        RuleFor(x => x.ExerciseId).NotEmpty();
        RuleFor(x => x.OrderIndex).GreaterThan(0);
        RuleFor(x => x.Sets).InclusiveBetween(1, 20);
        RuleFor(x => x.Reps).InclusiveBetween(0, 500);
        RuleFor(x => x.DurationSeconds).InclusiveBetween(1, 3600).When(x => x.DurationSeconds.HasValue);
    }
}
