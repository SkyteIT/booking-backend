using FluentValidation;

namespace Ube.Application.Features.Listings.Validators;

public class UpdateListingRequestValidator : AbstractValidator<UpdateListingRequest>
{
    public UpdateListingRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");

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

        // Which one of these must be present is determined by the chosen
        // Category's Type (checked in ListingService, which is the only place
        // that knows the category) - here we just validate whichever one was
        // actually sent.
        RuleFor(x => x.HotelDetails)
            .SetValidator(new HotelDetailsDtoValidator()!)
            .When(x => x.HotelDetails != null);

        RuleFor(x => x.RestaurantDetails)
            .SetValidator(new RestaurantDetailsDtoValidator()!)
            .When(x => x.RestaurantDetails != null);

        RuleFor(x => x.EventDetails)
            .SetValidator(new EventDetailsDtoValidator()!)
            .When(x => x.EventDetails != null);

        RuleFor(x => x.CarRentalDetails)
            .SetValidator(new CarRentalDetailsDtoValidator()!)
            .When(x => x.CarRentalDetails != null);

        RuleFor(x => x.ActivityDetails)
            .SetValidator(new ActivityDetailsDtoValidator()!)
            .When(x => x.ActivityDetails != null);
    }
}
