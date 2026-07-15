using FluentValidation;
using Ube.Domain.Enums.Listings;

namespace Ube.Application.Features.Listings.Validators;

public class CreateListingRequestValidator : AbstractValidator<CreateListingRequest>
{
    public CreateListingRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("A valid listing type is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .MaximumLength(10);

        RuleFor(x => x.Location)
            .MaximumLength(200);

        RuleFor(x => x.CancellationPolicy)
            .MaximumLength(1000);

        RuleFor(x => x.HotelDetails)
            .NotNull().WithMessage("Hotel details are required for a hotel listing")
            .SetValidator(new HotelDetailsDtoValidator()!)
            .When(x => x.Type == ListingType.Hotel);

        RuleFor(x => x.RestaurantDetails)
            .NotNull().WithMessage("Restaurant details are required for a restaurant listing")
            .SetValidator(new RestaurantDetailsDtoValidator()!)
            .When(x => x.Type == ListingType.Restaurant);

        RuleFor(x => x.EventDetails)
            .NotNull().WithMessage("Event details are required for an event listing")
            .SetValidator(new EventDetailsDtoValidator()!)
            .When(x => x.Type == ListingType.Event);

        RuleFor(x => x.CarRentalDetails)
            .NotNull().WithMessage("Car rental details are required for a car rental listing")
            .SetValidator(new CarRentalDetailsDtoValidator()!)
            .When(x => x.Type == ListingType.CarRental);

        RuleFor(x => x.ActivityDetails)
            .NotNull().WithMessage("Activity details are required for an activity listing")
            .SetValidator(new ActivityDetailsDtoValidator()!)
            .When(x => x.Type == ListingType.Activity);
    }
}
