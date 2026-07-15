using FluentValidation;

namespace Ube.Application.Features.Listings.Validators;

public class CarRentalDetailsDtoValidator : AbstractValidator<CarRentalDetailsDto>
{
    public CarRentalDetailsDtoValidator()
    {
        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Brand is required");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required");

        RuleFor(x => x.Transmission)
            .NotEmpty().WithMessage("Transmission is required");

        RuleFor(x => x.PricePerDay)
            .GreaterThan(0).WithMessage("Price per day must be greater than 0");

        RuleFor(x => x.SeatCount)
            .GreaterThan(0).WithMessage("Seat count must be greater than 0");

        RuleFor(x => x.FuelType)
            .NotEmpty().WithMessage("Fuel type is required");

        RuleFor(x => x.AvailabilityStatus)
            .NotEmpty().WithMessage("Availability status is required");

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
            .When(x => x.Year.HasValue)
            .WithMessage($"Year must be between 1900 and {DateTime.UtcNow.Year + 1}");

        RuleFor(x => x.HourlyRate)
            .GreaterThanOrEqualTo(0)
            .When(x => x.HourlyRate.HasValue)
            .WithMessage("Hourly rate cannot be negative");
    }
}
