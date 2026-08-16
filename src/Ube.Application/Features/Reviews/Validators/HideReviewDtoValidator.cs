using FluentValidation;

namespace Ube.Application.Features.Reviews.Validators;

public class HideReviewDtoValidator : AbstractValidator<HideReviewDto>
{
    public HideReviewDtoValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("A reason is required to hide a review")
            .MaximumLength(500);
    }
}
