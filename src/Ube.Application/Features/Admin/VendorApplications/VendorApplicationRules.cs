using Ube.Domain.Enums.Vendors;

namespace Ube.Application.Features.Admin.VendorApplications;

public static class VendorApplicationRules
{
    //Use fpr Result pattern to return success or failure with message
    public static Result CanReview(VendorApplicationStatus status)//Use fpr Result pattern to return success or failure with message
    {
        if (status != VendorApplicationStatus.Pending)
            return Result.Failure("Application already reviewed");       
        return Result.Success();
    }
//Use fpr Result pattern to return success or failure with message
    public static Result ValidateRejection(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure("Rejection reason is required");
        return Result.Success();
    }

    public static Result ValidateApproval(bool isAlreadyVendor, bool hasVendorProfile)
    {
        if (isAlreadyVendor)
            return Result.Failure("User is already a vendor");

        if (hasVendorProfile)
            return Result.Failure("Vendor profile already exists");

        return Result.Success();
    }
    public static Result CannotBeAdmin(Guid applicationUserId, Guid adminId)
    {
        if (applicationUserId == adminId)
            return Result.Failure("Admin cannot review their own application");
        return Result.Success();
    }
}