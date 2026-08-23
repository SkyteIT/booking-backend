using FluentValidation;

namespace Ube.Application.Features.Auth.Validators;

public class GoogleLoginRequestValidator : AbstractValidator<GoogleLoginRequest>
{
    public GoogleLoginRequestValidator()
    {
        RuleFor(x => x.EffectiveIdToken)
            .NotEmpty().WithMessage("Google ID token is required");
    }
}
