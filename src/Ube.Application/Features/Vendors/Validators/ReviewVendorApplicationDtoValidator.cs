using FluentValidation;
using Ube.Domain.Enums.Vendors;

namespace Ube.Application.Features.Vendors.Validators;

public class ReviewVendorApplicationDtoValidator : AbstractValidator<ReviewVendorApplicationDto>
{
    public ReviewVendorApplicationDtoValidator()
    {
        RuleFor(x => x.Status)
            .Must(s => s == VendorApplicationStatus.Approved || s == VendorApplicationStatus.Rejected)
            .WithMessage("Status must be Approved or Rejected");

        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage("Rejection reason is required when rejecting an application")
            .MaximumLength(500)
            .When(x => x.Status == VendorApplicationStatus.Rejected);
    }
}
