using FluentValidation;
using PhoneNumbers;

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
            .Matches(@"^(https?:\/\/)?([\w-]+\.)+[\w-]{2,}(\/\S*)?$")
            .When(x => !string.IsNullOrWhiteSpace(x.Website))
            .WithMessage("Website must be a valid URL");

        RuleFor(x => x.TaxId)
            .MaximumLength(100)
            .Matches(@"^[A-Za-z0-9\-]{4,100}$").WithMessage("Tax ID must be 4-100 alphanumeric characters")
            .When(x => !string.IsNullOrWhiteSpace(x.TaxId));

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
            .MaximumLength(20)
            .Must(BeValidSriLankanPhoneNumber)
            .WithMessage("Enter a valid Sri Lankan phone number, for example 0771234567 or +94771234567");

        RuleFor(x => x.Categories)
            .NotEmpty().WithMessage("At least one category is required");

        RuleFor(x => x.CurrentStep)
            .InclusiveBetween(1, 3).WithMessage("Step must be between 1 and 3");
    }

    private static bool BeValidSriLankanPhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;

        try
        {
            var util = PhoneNumberUtil.GetInstance();
            var parsed = util.Parse(phone.Trim(), "LK");
            return util.IsValidNumberForRegion(parsed, "LK");
        }
        catch (NumberParseException)
        {
            return false;
        }
    }
}
