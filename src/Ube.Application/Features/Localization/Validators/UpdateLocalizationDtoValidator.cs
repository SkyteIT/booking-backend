using FluentValidation;

namespace Ube.Application.Features.Localization.Validators;

public class UpdateLocalizationDtoValidator : AbstractValidator<UpdateLocalizationDto>
{
    public UpdateLocalizationDtoValidator()
    {
        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required")
            .MaximumLength(10);

        RuleFor(x => x.TimeZone)
            .NotEmpty().WithMessage("Time zone is required")
            .MaximumLength(100);

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code (e.g. USD, LKR)");
    }
}
