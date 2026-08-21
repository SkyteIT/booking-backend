using Xunit;
using Moq;
using Ube.Application.Features.Availability;
using Ube.Application.Features.Availability.Strategies;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Exceptions;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Bookings;
using Ube.Domain.Enums.Listings;

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
            bookingRepo.Object,
            new Mock<IUnitOfWork>().Object
        );
    }

    private static void SetupListingOwner(Mock<IListingRepository> listingRepo, Guid listingId, Guid vendorId)
    {
        var vendorProfileId = Guid.NewGuid();
        listingRepo
            .Setup(x => x.GetByIdAsync(listingId))
            .ReturnsAsync(new Listing
            {
                VendorProfileId = vendorProfileId,
                VendorProfile = new VendorProfile { Id = vendorProfileId, UserId = vendorId }
            });
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
                    EndDateTime = targetDate,
                    Status = BookingStatus.Confirmed
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

        Assert.Contains("already booked", exception.Message);
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

    [Fact]
    public async Task Calendar_Should_Show_Unbooked_Dates_As_Available_When_Legacy_Capacity_Is_Zero()
    {
        var ctx = CreateTestContext();
        var monthStart = DateTime.UtcNow.Date.AddMonths(1);
        var vendorProfileId = Guid.NewGuid();

        ctx.ListingRepo.Setup(x => x.GetByIdAsync(ctx.ListingId))
            .ReturnsAsync(new Listing
            {
                VendorProfileId = vendorProfileId,
                VendorProfile = new VendorProfile { Id = vendorProfileId, UserId = ctx.OwnerVendorId },
                AvailabilityType = AvailabilityType.Capacity,
                Capacity = 0
            });
        ctx.BlockedRepo.Setup(x => x.GetByListingAndDateRangeAsync(
                ctx.ListingId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<BlockedDate>());
        ctx.BookingRepo.Setup(x => x.GetBookingsByListingAndDateRangeAsync(
                ctx.ListingId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Booking>());

        var selector = new StrategySelector(new IAvailabilityStrategy[] { new CapacityStrategy() });
        var service = new AvailabilityService(
            ctx.BlockedRepo.Object,
            selector,
            ctx.ListingRepo.Object,
            ctx.BookingRepo.Object,
            new Mock<IUnitOfWork>().Object);

        var calendar = await service.GetCalanderAsync(
            ctx.ListingId, ctx.OwnerVendorId, monthStart.Month, monthStart.Year);

        Assert.All(calendar, day =>
        {
            Assert.Equal(Ube.Domain.Enums.AvailabilityStatus.Available, day.Status);
            Assert.Equal(1, day.AvailableCount);
            Assert.Equal(0, day.BookingCount);
        });
    }
}
