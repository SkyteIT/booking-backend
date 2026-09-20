using Moq;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Content.Category;
using Ube.Application.Features.Listings;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Vendors;

namespace Ube.Tests.Listings;

public class ListingCategoryAuthorizationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateAndUpdateRejectUnapprovedCategoryBeforeWriting(bool editing)
    {
        var userId = Guid.NewGuid();
        var vendorId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var listings = new Mock<IListingRepository>();
        var profiles = new Mock<IVendorProfileRepository>();
        var categories = new Mock<ICategoryRepository>();
        var transaction = new Mock<IUnitOfWork>();
        var permission = new Mock<IVendorListingCategoryService>();
        profiles.Setup(p => p.GetVendorIdAsync(userId)).ReturnsAsync(new VendorProfile { Id = vendorId });
        listings.Setup(l => l.GetByIdAsync(listingId)).ReturnsAsync(new Listing { Id = listingId, VendorProfileId = vendorId });
        permission.Setup(p => p.EnsureAllowedAsync(userId, categoryId, default)).ThrowsAsync(new BusinessRuleException("Category not approved"));
        var service = new ListingService(listings.Object, profiles.Object, categories.Object, transaction.Object, permission.Object);

        if (editing)
            await Assert.ThrowsAsync<BusinessRuleException>(() => service.UpdateListingAsync(listingId, userId, new UpdateListingRequest { CategoryId = categoryId }));
        else
            await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateListingAsync(userId, new CreateListingRequest { CategoryId = categoryId }));

        transaction.Verify(t => t.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public void PublicMapperPreservesHotelSelectionsAndVehicleType()
    {
        var hotel = ListingService.MapToResponse(new Listing {
            Type = Ube.Domain.Enums.Listings.ListingType.Hotel,
            HotelDetails = new HotelListingDetails { Amenities = "WiFi,Pool", RoomTypes = "Suite,Double", PrimaryRoomType = "Suite", PropertyType = "Resort" }
        });
        Assert.Equal(new[] { "WiFi", "Pool" }, hotel.HotelDetails!.Amenities);
        Assert.Equal("Suite", hotel.HotelDetails.PrimaryRoomType);
        var car = ListingService.MapToResponse(new Listing {
            Type = Ube.Domain.Enums.Listings.ListingType.CarRental,
            CarRentalDetails = new CarRentalListingDetails { VehicleType = "SUV", Transmission = "Automatic" }
        });
        Assert.Equal("SUV", car.CarRentalDetails!.VehicleType);
    }
}
