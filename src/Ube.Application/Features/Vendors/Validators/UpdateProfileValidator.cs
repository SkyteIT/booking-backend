using FluentValidation;


namespace Ube.Application.Features.Vendors.Validators;

public class UpdateProfileValidator : AbstractValidator<UpdateVendorProfileDto>
{
    public UpdateProfileValidator()
    {
        //First Name
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50);

        // Last Name
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50);

        // Phone
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\d{10}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Phone number must be 10 digits");

        // Business Name
        RuleFor(x => x.BusinessName)
            .NotEmpty().WithMessage("Business name is required")
            .MaximumLength(100);

        // Bio
        RuleFor(x => x.Bio)
            .MaximumLength(250);

        
    }
}