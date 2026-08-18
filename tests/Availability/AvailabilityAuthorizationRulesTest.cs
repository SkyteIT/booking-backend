using Xunit;
using Ube.Application.Features.Availability.rules;

using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Vendors;

namespace Ube.Tests.Availability;

public class AvailabilityAuthorizationRulesTests
{
    [Fact]
    public void Should_Succeed_When_Vendor_Owns_Listing()
    {
        var vendorId = Guid.NewGuid();

        var listing = new Listing
        {
            VendorProfileId = vendorId,
            VendorProfile = new VendorProfile { UserId = vendorId }
        };

        var result = AvailabilityAuthorizationRules
            .CanModifyAvailability(listing, vendorId);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Should_Fail_When_Vendor_Does_Not_Own_Listing()
    {
        var ownerVendorId = Guid.NewGuid();
        var listing = new Listing
        {
            VendorProfileId = ownerVendorId,
            VendorProfile = new VendorProfile { UserId = ownerVendorId }
        };

        var wrongVendorId = Guid.NewGuid();

        var result = AvailabilityAuthorizationRules
            .CanModifyAvailability(listing, wrongVendorId);

        Assert.False(result.IsSuccess);
        Assert.Contains("not allowed", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }
}