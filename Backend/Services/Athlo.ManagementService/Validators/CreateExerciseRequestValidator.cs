using Athlo.Models.DTOs.Exercises;
using Athlo.Shared.Validation;
using FluentValidation;

namespace Athlo.ManagementService.Validators;

public class CreateExerciseRequestValidator : AbstractValidator<CreateExerciseRequest>
{
    public CreateExerciseRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ImageUrl).MaximumLength(500).MustBeHttpsUrl();
    }
}
