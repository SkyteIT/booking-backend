using Ube.Domain.Entities.Listings;

namespace Ube.Application.Features.Availability.rules;

public static class AvailabilityAuthorizationRules
{
    // Check if listing belongs to vendor
    public static bool BelongsToVendor(Listing listing, Guid vendorId)
    {
        if (listing.VendorProfile != null)
        {
            return listing.VendorProfile.UserId == vendorId;
        }
        return listing.VendorProfileId == vendorId;
    }
    // Check if user can modify availability (listing must belong to vendor)
    public static Result CanModifyAvailability(Listing listing, Guid vendorId)
    {
        if (listing == null)
            return Result.Failure("Listing not found");

        if (listing.VendorProfile != null)
        {
            if (listing.VendorProfile.UserId != vendorId)
                return Result.Failure("You are not allowed to modify this listing");
        }
        else
        {
            if (listing.VendorProfileId != vendorId)
                return Result.Failure("You are not allowed to modify this listing");
        }

        return Result.Success();
    }
}