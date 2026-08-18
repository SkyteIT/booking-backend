using Xunit;
using Moq;
using Ube.Application.Features.Availability;
using Ube.Application.Features.Availability.rules;
using Ube.Application.Features.Availability.Strategies;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Bookings;

namespace Ube.Tests.Availability;

public class AvailabilityBlockingRulesTests
{
    // Test the date that already has a booking (should not be able to block)
    [Fact]
    public void WhenDateHasBooking()
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new Booking
            {
                StartDateTime = new DateTime(2027, 8, 15),
                EndDateTime = new DateTime(2027, 8, 15),
                Status = BookingStatus.Confirmed
            }
        };

        var dates = new List<DateTime>
        {
            new DateTime(2027, 8, 15)
        };

        // Act
        var result = AvailabilityBlockingRules.CanBlockDates(bookings, dates);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("already booked", result.ErrorMessage);
    }
    // Test the date that does not have a booking (should be able to block)
    [Fact]
    public void WhenNoBookingsExist()
    {
        // Arrange
        var bookings = new List<Booking>(); // no bookings

        var dates = new List<DateTime>
        {
            new DateTime(2027, 8, 20)
        };

        // Act
        var result = AvailabilityBlockingRules.CanBlockDates(bookings, dates);

        // Assert
        Assert.True(result.IsSuccess);
    }
    // Test multiple dates where some have bookings and some do not (should fail and indicate which date has booking)
    [Fact]
    public void Should_Fail_When_Some_Dates_Have_Bookings()
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new Booking
            {
                StartDateTime = new DateTime(2027, 8, 15),
                EndDateTime = new DateTime(2027, 8, 15)
            }
        };

        var dates = new List<DateTime>
        {
            new DateTime(2027, 8, 15), // booked ❌
            new DateTime(2027, 8, 20)  // free ✔
        };

        // Act
        var result = AvailabilityBlockingRules.CanBlockDates(bookings, dates);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("2027-08-15", result.ErrorMessage);
    }
    [Fact]
    public async Task Should_Block_Dates_Successfully_When_Valid()
    {
        // Arrange
        var bookingRepo = new Mock<IBookingRepository>();
        var listingRepo = new Mock<IListingRepository>();
        var blockedRepo = new Mock<IBlockedDateRepository>();

        var strategySelector = new StrategySelector(new List<IAvailabilityStrategy>());

        var listingId = Guid.NewGuid();
        var vendorId = Guid.NewGuid();

        listingRepo.Setup(x => x.GetByIdAsync(listingId))
            .ReturnsAsync(new Listing { VendorProfileId = vendorId });

        // no bookings
        bookingRepo.Setup(x => x.GetBookingsByListingAndDateRangeAsync(
                listingId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Booking>());

        // no existing blocked dates
        blockedRepo.Setup(x => x.GetByListingAndDatesAsync(
                listingId,
                It.IsAny<List<DateTime>>()))
            .ReturnsAsync(new List<BlockedDate>());

        var service = new AvailabilityService(
            blockedRepo.Object,
            strategySelector,
            listingRepo.Object,
            bookingRepo.Object,
            new Mock<IUnitOfWork>().Object
        );

        var dates = new List<DateTime>
        {
            new DateTime(2027, 8, 20)
        };

        // Act
        await service.BlockdatesAsync(listingId, vendorId, dates);

        // Assert
        blockedRepo.Verify(x => x.AddRangeAsync(It.IsAny<List<BlockedDate>>()), Times.Once);
    }
    [Fact]
    public async Task Should_Throw_When_Date_Is_Already_Blocked()
    {
        // Arrange
        var bookingRepo = new Mock<IBookingRepository>();
        var listingRepo = new Mock<IListingRepository>();
        var blockedRepo = new Mock<IBlockedDateRepository>();

        var strategySelector = new StrategySelector(new List<IAvailabilityStrategy>());

        var listingId = Guid.NewGuid();
        var vendorId = Guid.NewGuid();

        listingRepo.Setup(x => x.GetByIdAsync(listingId))
            .ReturnsAsync(new Listing { VendorProfileId = vendorId });

        // no bookings
        bookingRepo.Setup(x => x.GetBookingsByListingAndDateRangeAsync(
                listingId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Booking>());

        // simulate already blocked date
        blockedRepo.Setup(x => x.GetByListingAndDatesAsync(
                listingId,
                It.IsAny<List<DateTime>>()))
            .ReturnsAsync(new List<BlockedDate>
            {
                new BlockedDate
                {
                    ListingId = listingId,
                    Date = new DateTime(2027, 8, 20)
                }
            });

        var service = new AvailabilityService(
            blockedRepo.Object,
            strategySelector,
            listingRepo.Object,
            bookingRepo.Object,
            new Mock<IUnitOfWork>().Object
        );

        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.BlockdatesAsync(
                listingId,
                vendorId,
                new List<DateTime> { new DateTime(2027,8,20) }
            )
        );

        // Assert
        Assert.Contains("already blocked", exception.Message, StringComparison.OrdinalIgnoreCase);

        blockedRepo.Verify(x => x.AddRangeAsync(It.IsAny<List<BlockedDate>>()), Times.Never);
    }

    // UnblockDatesAsync

    //Unblock dates successfully when valid
    [Fact]
    public async Task Should_Unblock_Dates_Successfully_When_Valid()
    {
        // Arrange
        var bookingRepo = new Mock<IBookingRepository>();
        var listingRepo = new Mock<IListingRepository>();
        var blockedRepo = new Mock<IBlockedDateRepository>();

        var strategySelector = new StrategySelector(new List<IAvailabilityStrategy>());

        var listingId = Guid.NewGuid();
        var vendorId = Guid.NewGuid();

        listingRepo.Setup(x => x.GetByIdAsync(listingId))
            .ReturnsAsync(new Listing { VendorProfileId = vendorId });

        // simulate existing blocked dates
        blockedRepo.Setup(x => x.GetByListingAndDatesAsync(
                listingId,
                It.IsAny<List<DateTime>>()))
            .ReturnsAsync(new List<BlockedDate>
            {
                new BlockedDate
                {
                    ListingId = listingId,
                    Date = new DateTime(2027,8,20)
                }
            });

        var service = new AvailabilityService(
            blockedRepo.Object,
            strategySelector,
            listingRepo.Object,
            bookingRepo.Object,
            new Mock<IUnitOfWork>().Object
        );

        var dates = new List<DateTime>
        {
            new DateTime(2027,8,20)
        };

        // Act
        await service.UnBlockdatesAsync(listingId, vendorId, dates);

        // Assert
        blockedRepo.Verify(x => x.RemoveRangeAsync(It.IsAny<List<BlockedDate>>()), Times.Once);
    }
    // Unblock date that  not blocked (should not throw, just do nothing)
    [Fact]
public async Task Should_Throw_When_Unblocking_NonBlocked_Dates()
{
    // Arrange
    var bookingRepo = new Mock<IBookingRepository>();
    var listingRepo = new Mock<IListingRepository>();
    var blockedRepo = new Mock<IBlockedDateRepository>();

    var strategySelector = new StrategySelector(new List<IAvailabilityStrategy>());

    var listingId = Guid.NewGuid();
    var vendorId = Guid.NewGuid();

    listingRepo.Setup(x => x.GetByIdAsync(listingId))
        .ReturnsAsync(new Listing { VendorProfileId = vendorId });

    // simulate NO blocked dates
    blockedRepo.Setup(x => x.GetByListingAndDatesAsync(
            listingId,
            It.IsAny<List<DateTime>>()))
        .ReturnsAsync(new List<BlockedDate>());

    var service = new AvailabilityService(
        blockedRepo.Object,
        strategySelector,
        listingRepo.Object,
        bookingRepo.Object,
        new Mock<IUnitOfWork>().Object
    );

    var dates = new List<DateTime>
    {
        new DateTime(2027,8,20)
    };

    // Act
    var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
        service.UnBlockdatesAsync(listingId, vendorId, dates)
    );

    // Assert
    Assert.Contains("not blocked", exception.Message, StringComparison.OrdinalIgnoreCase);

    blockedRepo.Verify(x => x.RemoveRangeAsync(It.IsAny<List<BlockedDate>>()), Times.Never);
}
}