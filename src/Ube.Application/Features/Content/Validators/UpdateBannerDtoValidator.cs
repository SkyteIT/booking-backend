using FluentValidation;

namespace Ube.Application.Features.Content.Validators;

public class UpdateBannerDtoValidator : AbstractValidator<UpdateBannerDto>
{
    public UpdateBannerDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200);

        RuleFor(x => x.Subtitle)
            .MaximumLength(300)
            .When(x => !string.IsNullOrWhiteSpace(x.Subtitle));

        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("Image URL is required")
            .MaximumLength(500);

        RuleFor(x => x.Placement)
            .GreaterThanOrEqualTo(0).WithMessage("Placement is required");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");
    }
}
