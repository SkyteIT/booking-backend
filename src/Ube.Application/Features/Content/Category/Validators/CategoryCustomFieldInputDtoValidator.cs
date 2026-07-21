using FluentValidation;
using Ube.Domain.Enums.Listings;

namespace Ube.Application.Features.Content.Category.Validators;

public class CategoryCustomFieldInputDtoValidator : AbstractValidator<CategoryCustomFieldInputDto>
{
    public CategoryCustomFieldInputDtoValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Field label is required")
            .MaximumLength(100);

        RuleFor(x => x.FieldType)
            .IsInEnum().WithMessage("A valid field type is required");

        RuleFor(x => x.Options)
            .Must(o => o != null && o.Count > 0)
            .WithMessage("A Dropdown field needs at least one option")
            .When(x => x.FieldType == CustomFieldType.Dropdown);
    }
}
