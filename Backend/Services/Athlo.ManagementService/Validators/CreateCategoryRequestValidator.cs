using Athlo.Models.DTOs.Programs;
using FluentValidation;

namespace Athlo.ManagementService.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(50)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage("Slug must be lowercase letters, numbers, and hyphens.");
        RuleFor(x => x.Icon).NotEmpty().MaximumLength(50);
    }
}
