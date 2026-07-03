using Athlo.Models.DTOs.Programs;
using Athlo.Shared.Validation;
using FluentValidation;

namespace Athlo.ManagementService.Validators;

public class CreateProgramRequestValidator : AbstractValidator<CreateProgramRequest>
{
    public CreateProgramRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.ImageUrl).MaximumLength(500).MustBeHttpsUrl();
        RuleFor(x => x.Difficulty).IsInEnum().WithMessage("Invalid difficulty level.");
        RuleFor(x => x.DurationMinutes).InclusiveBetween(5, 300);
        RuleFor(x => x.EstimatedCalories).InclusiveBetween(50, 5000);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Exercises).NotEmpty();
        RuleForEach(x => x.Exercises).SetValidator(new ProgramExerciseInputValidator());
    }
}
