using Athlo.Models.DTOs.Workouts;
using FluentValidation;

namespace Athlo.ManagementService.Validators;

public class CancelWorkoutRequestValidator : AbstractValidator<CancelWorkoutRequest>
{
    public CancelWorkoutRequestValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
    }
}
