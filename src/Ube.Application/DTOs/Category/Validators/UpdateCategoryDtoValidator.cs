using FluentValidation;

namespace Ube.Application.DTOs.Category.Validators;

public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
{
    private static readonly string[] ValidStatuses = ["Active", "Inactive", "Deleted"];

    public UpdateCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => x.Name != null);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description != null);

        RuleFor(x => x.DefaultCommissionPercent)
            .InclusiveBetween(0, 100).WithMessage("Commission percent must be between 0 and 100")
            .When(x => x.DefaultCommissionPercent.HasValue);

        RuleFor(x => x.PlatformServiceFee)
            .GreaterThanOrEqualTo(0).WithMessage("Platform service fee cannot be negative")
            .When(x => x.PlatformServiceFee.HasValue);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(1).WithMessage("Display order must be at least 1")
            .When(x => x.DisplayOrder.HasValue);

        RuleFor(x => x.Status)
            .Must(s => ValidStatuses.Contains(s!))
            .WithMessage("Status must be Active, Inactive or Deleted")
            .When(x => x.Status != null);
    }
}
