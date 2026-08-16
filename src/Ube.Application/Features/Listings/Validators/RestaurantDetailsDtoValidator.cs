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
            .NotEmpty().WithMessage("Opening hours are required");

        RuleFor(x => x.TableCapacity)
            .GreaterThan(0).WithMessage("Table capacity must be greater than 0");
    }
}
