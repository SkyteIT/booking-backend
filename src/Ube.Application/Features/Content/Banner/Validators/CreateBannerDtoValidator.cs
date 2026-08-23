using FluentValidation;
using Ube.Domain.Enums;
using Ube.Domain.Enums.Content;

namespace Ube.Application.Features.Content.Banner.Validators;

public class CreateBannerDtoValidator : AbstractValidator<CreateBannerDto>
{
    public CreateBannerDtoValidator()
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
            .Must(value => Enum.IsDefined(typeof(BannerPlacement), value))
            .WithMessage("Placement is required");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Display order must be zero or a positive number");

        RuleFor(x => x.Status)
            .Must(value => Enum.IsDefined(typeof(RecordStatus), value))
            .WithMessage("Status is required");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");
    }
}
