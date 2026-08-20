using FluentValidation;

namespace Ube.Application.Features.Vendors.Validators;

public class ReviewVendorApplicationDtoValidator : AbstractValidator<ReviewVendorApplicationDto>
{
    public ReviewVendorApplicationDtoValidator()
    {
        RuleFor(x => x.Status)
            .Must(s => string.Equals(s, "Approved", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(s, "Rejected", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Status must be Approved or Rejected");

        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage("Rejection reason is required when rejecting an application")
            .MaximumLength(500)
            .When(x => string.Equals(x.Status, "Rejected", StringComparison.OrdinalIgnoreCase));
    }
}
