using Athlo.Models.DTOs.Workouts;
using FluentValidation;

namespace Athlo.ManagementService.Validators;

public class StartWorkoutRequestValidator : AbstractValidator<StartWorkoutRequest>
{
    public StartWorkoutRequestValidator()
    {
        RuleFor(x => x.ProgramId).NotEmpty();
    }
}
