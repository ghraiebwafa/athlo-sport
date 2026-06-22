using Athlo.Models.DTOs.Workouts;
using FluentValidation;

namespace Athlo.ManagementService.Validators;

public class CompleteWorkoutRequestValidator : AbstractValidator<CompleteWorkoutRequest>
{
    public CompleteWorkoutRequestValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.CaloriesBurned).GreaterThanOrEqualTo(0);
    }
}
