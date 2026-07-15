using FluentValidation;

namespace Ube.Application.Features.Content.Promotion.Validators;

public class UpdatePromotionDtoValidator : AbstractValidator<UpdatePromotionDto>
{
    public UpdatePromotionDtoValidator()
    {
        RuleFor(x => x.PromoCode)
            .NotEmpty().WithMessage("Promo code is required")
            .MaximumLength(50)
            .Matches(@"^[A-Z0-9_\-]+$").WithMessage("Promo code must be uppercase letters, numbers, hyphens or underscores only");

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("Discount value must be greater than 0");

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0).WithMessage("Usage limit must be greater than 0")
            .When(x => x.UsageLimit.HasValue);

        RuleFor(x => x.UsageCount)
            .GreaterThanOrEqualTo(0).WithMessage("Usage count cannot be negative");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");
    }
}
