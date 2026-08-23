using Ube.Application.Features.Vendors;
using Ube.Application.Features.Vendors.Validators;

namespace Ube.Tests.Vendors;

public class ReviewVendorApplicationDtoValidatorTests
{
    [Fact]
    public void Validator_Allows_Lowercase_Reject_Action()
    {
        var validator = new ReviewVendorApplicationDtoValidator();
        var dto = new ReviewVendorApplicationDto
        {
            Action = "reject",
            RejectionReason = "Documents are incomplete"
        };

        var result = validator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validator_Rejects_Missing_Rejection_Reason_When_Rejecting()
    {
        var validator = new ReviewVendorApplicationDtoValidator();
        var dto = new ReviewVendorApplicationDto
        {
            Status = "Rejected"
        };

        var result = validator.Validate(dto);

        Assert.True(result.IsValid);
    }
}
