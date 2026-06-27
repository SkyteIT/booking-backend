using FluentValidation;

namespace Ube.Application.Features.Bookings.Validators;

public class VendorRegisterApplicationDtoValidator : AbstractValidator<VendorRegisterApplicationDto>
{
    public VendorRegisterApplicationDtoValidator()
    {
        RuleFor(x => x.BusinessName)
            .NotEmpty().WithMessage("Business name is required")
            .MaximumLength(200);

        RuleFor(x => x.BusinessType)
            .NotEmpty().WithMessage("Business type is required")
            .MaximumLength(200);

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(500);

        RuleFor(x => x.Website)
            .MaximumLength(300)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Website))
            .WithMessage("Website must be a valid URL");

        RuleFor(x => x.TaxId)
            .MaximumLength(100);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(256);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+?[0-9\s\-]{7,20}$").WithMessage("Phone number is not valid")
            .MaximumLength(20);

        RuleFor(x => x.Categories)
            .NotEmpty().WithMessage("At least one category is required");

        RuleFor(x => x.CurrentStep)
            .InclusiveBetween(1, 3).WithMessage("Step must be between 1 and 3");
    }
}
