using FluentValidation;

namespace Ube.Application.Features.Content.Category.Validators;

public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    private static readonly string[] ValidStatuses = ["Active", "Inactive"];

    public CreateCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.DefaultCommissionPercent)
            .InclusiveBetween(0, 100).WithMessage("Commission percent must be between 0 and 100");

        RuleFor(x => x.PlatformServiceFee)
            .GreaterThanOrEqualTo(0).WithMessage("Platform service fee cannot be negative")
            .When(x => x.PlatformServiceFee.HasValue);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(1).WithMessage("Display order must be at least 1");

        RuleFor(x => x.Status)
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage("Status must be Active or Inactive");
    }
}
