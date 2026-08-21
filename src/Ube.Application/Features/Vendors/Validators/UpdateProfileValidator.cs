using FluentValidation;
using PhoneNumbers;

namespace Ube.Application.Features.Vendors.Validators;

public class UpdateProfileValidator : AbstractValidator<UpdateVendorProfileDto>
{
    public UpdateProfileValidator()
    {
        // First Name
        RuleFor(x => x.FirstName)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("First name is required")
            .MaximumLength(50)
            .WithMessage("First name must not exceed 50 characters");

        // Last Name
        RuleFor(x => x.LastName)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Last name is required")
            .MaximumLength(50)
            .WithMessage("Last name must not exceed 50 characters");

        // Phone Number - validate international phone numbers using libphonenumber
        RuleFor(x => x.PhoneNumber)
            .Must(phone =>
            {
                if (string.IsNullOrWhiteSpace(phone)) return true;
                // Require international format starting with + to avoid needing a default region
                if (!phone.StartsWith("+")) return false;
                try
                {
                    var util = PhoneNumberUtil.GetInstance();
                    var parsed = util.Parse(phone, null);
                    return util.IsValidNumber(parsed);
                }
                catch
                {
                    return false;
                }
            })
            .WithMessage("Phone number must be a valid international phone number (E.164), starting with + and country code");

        // Business Name
        RuleFor(x => x.BusinessName)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Business name is required")
            .MaximumLength(100)
            .WithMessage("Business name must not exceed 100 characters");

        // Bio
        RuleFor(x => x.Bio)
            .MaximumLength(250)
            .When(x => !string.IsNullOrEmpty(x.Bio))
            .WithMessage("Bio must not exceed 250 characters");
    }
}