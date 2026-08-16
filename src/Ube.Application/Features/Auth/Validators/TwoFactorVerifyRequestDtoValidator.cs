using FluentValidation;

namespace Ube.Application.Features.Auth.Validators;

public class TwoFactorVerifyRequestDtoValidator : AbstractValidator<TwoFactorVerifyRequestDto>
{
    public TwoFactorVerifyRequestDtoValidator()
    {
        RuleFor(x => x.ChallengeToken)
            .NotEmpty().WithMessage("Challenge token is required");

        // Accepts either a 6-digit TOTP code or a 10-character hex backup
        // code - actual matching logic (which kind it is) happens in the
        // service, this just guards against empty/malformed input.
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required")
            .Matches("^[0-9A-Fa-f]{6,10}$").WithMessage("Enter a valid 6-digit code or backup code");
    }
}
