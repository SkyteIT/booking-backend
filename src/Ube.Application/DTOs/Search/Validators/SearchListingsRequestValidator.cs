using FluentValidation;

namespace Ube.Application.DTOs.Search.Validators;

public class SearchListingsRequestValidator : AbstractValidator<SearchListingsRequest>
{
    public SearchListingsRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50).WithMessage("Page size must be between 1 and 50");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum price cannot be negative")
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum price cannot be negative")
            .GreaterThanOrEqualTo(x => x.MinPrice!.Value).WithMessage("Maximum price must be greater than minimum price")
            .When(x => x.MaxPrice.HasValue && x.MinPrice.HasValue);

        RuleFor(x => x.MinRating)
            .InclusiveBetween(0, 5).WithMessage("Rating must be between 0 and 5")
            .When(x => x.MinRating.HasValue);

        RuleFor(x => x.SearchTerm)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm));
    }
}
