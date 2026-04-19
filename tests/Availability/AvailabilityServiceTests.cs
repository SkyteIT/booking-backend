using Xunit;
using Moq;
using Ube.Application.Features.Availability;
using Ube.Application.Features.Availability.Strategies;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Exceptions;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Listings;

namespace Ube.Tests.Availability;

public class AvailabilityServiceTests
{
    private const int BookingConflictOffsetDays = 10;
    private const int UnauthorizedOffsetDays = 12;

    private sealed record TestContext(
        Mock<IBookingRepository> BookingRepo,
        Mock<IListingRepository> ListingRepo,
        Mock<IBlockedDateRepository> BlockedRepo,
        Guid ListingId,
        Guid OwnerVendorId,
        Guid OtherVendorId);

    private static StrategySelector CreateStrategySelector() =>
        new(new List<IAvailabilityStrategy>());

    private static DateTime CreateFutureDate(int offsetDays) =>
        DateTime.UtcNow.Date.AddDays(offsetDays);

    private static TestContext CreateTestContext()
    {
        return new TestContext(
            new Mock<IBookingRepository>(),
            new Mock<IListingRepository>(),
            new Mock<IBlockedDateRepository>(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());
    }

    private static AvailabilityService CreateService(
        Mock<IBlockedDateRepository> blockedRepo,
        Mock<IListingRepository> listingRepo,
        Mock<IBookingRepository> bookingRepo)
    {
        return new AvailabilityService(
            blockedRepo.Object,
            CreateStrategySelector(),
            listingRepo.Object,
            bookingRepo.Object
        );
    }

    private static void SetupListingOwner(Mock<IListingRepository> listingRepo, Guid listingId, Guid vendorId)
    {
        listingRepo
            .Setup(x => x.GetByIdAsync(listingId))
            .ReturnsAsync(new Listing { VendorProfileId = vendorId });
    }

    private static void SetupNoBlockedDates(Mock<IBlockedDateRepository> blockedRepo, Guid listingId)
    {
        blockedRepo
            .Setup(x => x.GetByListingAndDatesAsync(listingId, It.IsAny<List<DateTime>>()))
            .ReturnsAsync(new List<BlockedDate>());
    }

    [Fact]
    public async Task WhenDateHasBooking()
    {
        // Arrange
        var ctx = CreateTestContext();
        var targetDate = CreateFutureDate(BookingConflictOffsetDays);

        SetupListingOwner(ctx.ListingRepo, ctx.ListingId, ctx.OwnerVendorId);
        SetupNoBlockedDates(ctx.BlockedRepo, ctx.ListingId);

        // Booking exists on the requested blocked date.
        ctx.BookingRepo.Setup(x => x.GetBookingsByListingAndDateRangeAsync(
                ctx.ListingId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Booking>
            {
                new Booking
                {
                    StartDateTime = targetDate,
                    EndDateTime = targetDate
                }
            });

        var service = CreateService(ctx.BlockedRepo, ctx.ListingRepo, ctx.BookingRepo);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.BlockdatesAsync(
                ctx.ListingId,
                ctx.OwnerVendorId,
                new List<DateTime> { targetDate }
            )
        );

        Assert.Contains("already has a booking", exception.Message);
    }

    [Fact]
    public async Task Should_Throw_When_Vendor_Does_Not_Own_Listing()
    {
        // Arrange
        var ctx = CreateTestContext();
        var targetDate = CreateFutureDate(UnauthorizedOffsetDays);

        // listing belongs to someone else
        SetupListingOwner(ctx.ListingRepo, ctx.ListingId, ctx.OwnerVendorId);

        var service = CreateService(ctx.BlockedRepo, ctx.ListingRepo, ctx.BookingRepo);

        // Act
        var exception = await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.BlockdatesAsync(
                ctx.ListingId,
                ctx.OtherVendorId,
                new List<DateTime> { targetDate }
            )
        );

        // Assert
        Assert.Contains("not allowed", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}