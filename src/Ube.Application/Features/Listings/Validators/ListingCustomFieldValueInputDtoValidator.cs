using FluentValidation;

namespace Ube.Application.Features.Listings.Validators;

public class ListingCustomFieldValueInputDtoValidator : AbstractValidator<ListingCustomFieldValueInputDto>
{
    public ListingCustomFieldValueInputDtoValidator()
    {
        RuleFor(x => x.CategoryCustomFieldId)
            .NotEmpty().WithMessage("CategoryCustomFieldId is required");

        RuleFor(x => x.Value)
            .MaximumLength(1000);
    }
}
