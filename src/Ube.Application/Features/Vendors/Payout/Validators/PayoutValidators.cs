using FluentValidation;

namespace Ube.Application.Features.Vendors.Payout.Validators;
public class UpdateVendorPayoutValidator : AbstractValidator<UpdateVendorPayoutDto>
{
    public UpdateVendorPayoutValidator()
    {
        RuleFor(x => x.BankName)
            .NotEmpty().WithMessage("Bank name is required")
            .MaximumLength(100);

        RuleFor(x => x.AccountHolderName)
            .NotEmpty().WithMessage("Account holder name is required")
            .MaximumLength(100);

        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("Account number is required")
            .Matches(@"^\d{8,20}$")
            .WithMessage("Account number must be 8–20 digits");

        RuleFor(x => x.Branch)
            .MaximumLength(100);
    }
}