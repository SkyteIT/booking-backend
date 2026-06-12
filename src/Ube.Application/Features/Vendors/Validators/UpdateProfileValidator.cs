using FluentValidation;

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

        // Phone Number
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?\d{10,15}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Phone number must be valid (10–15 digits, optional +)");

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