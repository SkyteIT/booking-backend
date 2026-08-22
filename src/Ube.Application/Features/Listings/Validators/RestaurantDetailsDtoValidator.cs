using FluentValidation;

namespace Ube.Application.Features.Listings.Validators;

public class RestaurantDetailsDtoValidator : AbstractValidator<RestaurantDetailsDto>
{
    public RestaurantDetailsDtoValidator()
    {
        RuleFor(x => x.CuisineType)
            .NotEmpty().WithMessage("Cuisine type is required");

        RuleFor(x => x.AverageCost)
            .GreaterThan(0).WithMessage("Average cost must be greater than 0");

        RuleFor(x => x.OpeningHours)
            .NotEmpty().WithMessage("Opening hours are required")
            .Matches(ListingTimeFormat.TimeOrRangePattern)
            .WithMessage("Opening hours must use 12-hour AM/PM format, for example 9:00 AM - 10:00 PM")
            .When(x => string.IsNullOrWhiteSpace(x.OpeningTime)
                    && string.IsNullOrWhiteSpace(x.ClosingTime));

        RuleFor(x => x.OpeningTime)
            .NotEmpty().WithMessage("Opening time is required")
            .Matches(ListingTimeFormat.NumericTimePattern)
            .WithMessage("Opening time must contain numbers in 12-hour format, for example 9:00")
            .When(x => !string.IsNullOrWhiteSpace(x.OpeningTime)
                    || !string.IsNullOrWhiteSpace(x.ClosingTime));

        RuleFor(x => x.OpeningPeriod)
            .NotEmpty().WithMessage("Select AM or PM for opening time")
            .Matches(ListingTimeFormat.PeriodPattern)
            .WithMessage("Opening period must be AM or PM")
            .When(x => !string.IsNullOrWhiteSpace(x.OpeningTime)
                    || !string.IsNullOrWhiteSpace(x.ClosingTime));

        RuleFor(x => x.ClosingTime)
            .NotEmpty().WithMessage("Closing time is required")
            .Matches(ListingTimeFormat.NumericTimePattern)
            .WithMessage("Closing time must contain numbers in 12-hour format, for example 10:00")
            .When(x => !string.IsNullOrWhiteSpace(x.OpeningTime)
                    || !string.IsNullOrWhiteSpace(x.ClosingTime));

        RuleFor(x => x.ClosingPeriod)
            .NotEmpty().WithMessage("Select AM or PM for closing time")
            .Matches(ListingTimeFormat.PeriodPattern)
            .WithMessage("Closing period must be AM or PM")
            .When(x => !string.IsNullOrWhiteSpace(x.OpeningTime)
                    || !string.IsNullOrWhiteSpace(x.ClosingTime));

        RuleFor(x => x.TableCapacity)
            .GreaterThan(0).WithMessage("Table capacity must be greater than 0");
    }
}
