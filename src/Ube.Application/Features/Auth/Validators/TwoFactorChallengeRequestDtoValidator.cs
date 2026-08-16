using FluentValidation;

namespace Ube.Application.Features.Auth.Validators;

public class TwoFactorChallengeRequestDtoValidator : AbstractValidator<TwoFactorChallengeRequestDto>
{
    public TwoFactorChallengeRequestDtoValidator()
    {
        RuleFor(x => x.ChallengeToken)
            .NotEmpty().WithMessage("Challenge token is required");
    }
}
