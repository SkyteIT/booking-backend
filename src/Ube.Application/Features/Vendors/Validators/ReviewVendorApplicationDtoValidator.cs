using FluentValidation;

namespace Ube.Application.Features.Vendors.Validators;

public class ReviewVendorApplicationDtoValidator : AbstractValidator<ReviewVendorApplicationDto>
{
    public ReviewVendorApplicationDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => HasValidDecision(x.Status, x.Action))
            .WithMessage("Status or action must be approve/Approved or reject/Rejected");

        RuleFor(x => x.RejectionReason)
            .MaximumLength(500)
            .When(x => IsRejectionDecision(x.Status, x.Action));
    }

    private static bool HasValidDecision(string? status, string? action)
    {
        return IsApprovalDecision(status, action) || IsRejectionDecision(status, action);
    }

    private static bool IsApprovalDecision(string? status, string? action)
    {
        return IsMatch(status, "Approved", "approve") || IsMatch(action, "Approved", "approve");
    }

    private static bool IsRejectionDecision(string? status, string? action)
    {
        return IsMatch(status, "Rejected", "reject") || IsMatch(action, "Rejected", "reject");
    }

    private static bool IsMatch(string? value, params string[] acceptedValues)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return acceptedValues.Any(accepted =>
            string.Equals(value.Trim(), accepted, StringComparison.OrdinalIgnoreCase));
    }
}
