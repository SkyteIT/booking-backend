using FluentValidation;

namespace Ube.Application.Features.Listings.Validators;

public class ActivityDetailsDtoValidator : AbstractValidator<ActivityDetailsDto>
{
    public ActivityDetailsDtoValidator()
    {
        RuleFor(x => x.ActivityType)
            .NotEmpty().WithMessage("Activity type is required");

        RuleFor(x => x.DurationHours)
            .GreaterThan(0).WithMessage("Duration must be greater than 0 hours");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Price.HasValue)
            .WithMessage("Price cannot be negative");

        RuleFor(x => x.MinGroupSize)
            .GreaterThan(0)
            .When(x => x.MinGroupSize.HasValue)
            .WithMessage("Minimum group size must be greater than 0");

        RuleFor(x => x.MaxGroupSize)
            .GreaterThanOrEqualTo(x => x.MinGroupSize ?? 0)
            .When(x => x.MaxGroupSize.HasValue)
            .WithMessage("Maximum group size must be greater than or equal to minimum group size");

        RuleFor(x => x.MinAge)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinAge.HasValue)
            .WithMessage("Minimum age cannot be negative");

        RuleFor(x => x.MaxAge)
            .GreaterThanOrEqualTo(x => x.MinAge ?? 0)
            .When(x => x.MaxAge.HasValue)
            .WithMessage("Maximum age must be greater than or equal to minimum age");
    }
}
