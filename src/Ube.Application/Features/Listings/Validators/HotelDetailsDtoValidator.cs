using FluentValidation;

namespace Ube.Application.Features.Listings.Validators;

public class HotelDetailsDtoValidator : AbstractValidator<HotelDetailsDto>
{
    public HotelDetailsDtoValidator()
    {
        RuleFor(x => x.PricePerNight)
            .GreaterThan(0).WithMessage("Price per night must be greater than 0");

        RuleFor(x => x.AvailableRooms)
            .GreaterThanOrEqualTo(0).WithMessage("Available rooms cannot be negative");

        RuleFor(x => x.CheckInTime)
            .NotEmpty().WithMessage("Check-in time is required");

        RuleFor(x => x.CheckOutTime)
            .NotEmpty().WithMessage("Check-out time is required");

        RuleFor(x => x.Amenities)
            .NotEmpty().WithMessage("At least one amenity is required");

        RuleFor(x => x.RoomTypes)
            .NotEmpty().WithMessage("At least one room type is required");
    }
}
