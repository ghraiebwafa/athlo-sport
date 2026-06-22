using Athlo.Models.DTOs.Auth;
using FluentValidation;

namespace Athlo.AuthService.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
