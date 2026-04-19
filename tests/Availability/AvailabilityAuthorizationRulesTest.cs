using Xunit;
using Ube.Application.Features.Availability.rules;

using Ube.Domain.Entities.Listings;

namespace Ube.Tests.Availability;

public class AvailabilityAuthorizationRulesTests
{
    [Fact]
    public void Should_Succeed_When_Vendor_Owns_Listing()
    {
        var vendorId = Guid.NewGuid();

        var listing = new Listing
        {
            VendorProfileId = vendorId
        };

        var result = AvailabilityAuthorizationRules
            .CanModifyAvailability(listing, vendorId);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Should_Fail_When_Vendor_Does_Not_Own_Listing()
    {
        var listing = new Listing
        {
            VendorProfileId = Guid.NewGuid()
        };

        var wrongVendorId = Guid.NewGuid();

        var result = AvailabilityAuthorizationRules
            .CanModifyAvailability(listing, wrongVendorId);

        Assert.False(result.IsSuccess);
        Assert.Contains("not allowed", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }
}