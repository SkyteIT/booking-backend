using Moq;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Dashboard;
using Ube.Application.Features.Payments;
using Ube.Application.Features.Reviews;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Bookings;

namespace Ube.Tests.Dashboard;

public class VendorDashboardServiceTests
{
    private sealed record Ctx(
        Mock<IBookingRepository> BookingRepo,
        Mock<IListingRepository> ListingRepo,
        Mock<IReviewRepository> ReviewRepo,
        Mock<IVendorRevenueReportService> RevenueReportService,
        VendorDashboardService Service);

    private static Ctx Build()
    {
        var bookingRepo = new Mock<IBookingRepository>();
        var listingRepo = new Mock<IListingRepository>();
        var reviewRepo = new Mock<IReviewRepository>();
        var revenueReportService = new Mock<IVendorRevenueReportService>();
        return new Ctx(
            bookingRepo, listingRepo, reviewRepo, revenueReportService,
            new VendorDashboardService(bookingRepo.Object, listingRepo.Object, reviewRepo.Object, revenueReportService.Object));
    }

    private static Booking MakeBooking(Guid listingId, string listingTitle, BookingStatus status, decimal amount, DateTime createdAt) =>
        new Booking
        {
            Id = Guid.NewGuid(),
            BookingNumber = "BKG-000001",
            CustomerId = Guid.NewGuid(),
            ListingId = listingId,
            Status = status,
            TotalAmount = amount,
            Currency = "LKR",
            CreatedAt = createdAt,
            StartDateTime = createdAt,
            EndDateTime = createdAt.AddDays(1),
            Listing = new Listing
            {
                Id = listingId,
                Title = listingTitle,
                VendorProfile = new VendorProfile { UserId = Guid.NewGuid() }
            }
        };

    [Fact]
    public async Task GetVendorEarnings_Computes_StatusBreakdown_Within_Range()
    {
        var ctx = Build();
        var vendorUserId = Guid.NewGuid();
        var vendorProfileId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var inRange = new DateTime(2026, 3, 10);
        var outOfRange = new DateTime(2026, 1, 1);

        var bookings = new List<Booking>
        {
            MakeBooking(listingId, "Room A", BookingStatus.Confirmed, 1000, inRange),
            MakeBooking(listingId, "Room A", BookingStatus.Completed, 1000, inRange),
            MakeBooking(listingId, "Room A", BookingStatus.Pending, 500, inRange),
            MakeBooking(listingId, "Room A", BookingStatus.Cancelled, 500, outOfRange), // excluded by date
        };

        ctx.BookingRepo.Setup(r => r.GetAllBookingsByVendorIdAsync(vendorUserId)).ReturnsAsync(bookings);
        ctx.RevenueReportService
            .Setup(s => s.GetGrossCommissionRefundBreakdownAsync(vendorProfileId, It.IsAny<RevenueReportRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GrossCommissionRefundBreakdownDto());
        ctx.RevenueReportService
            .Setup(s => s.GetReportAsync(vendorProfileId, It.IsAny<RevenueReportRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RevenuePeriodDto>());

        var request = new RevenueReportRequest { StartDate = inRange.AddDays(-1), EndDate = inRange.AddDays(1) };
        var result = await ctx.Service.GetVendorEarningsAsync(vendorUserId, vendorProfileId, request);

        Assert.Equal(3, result.StatusBreakdown.Total); // the out-of-range one excluded
        Assert.Equal(1, result.StatusBreakdown.Pending);
        Assert.Equal(1, result.StatusBreakdown.Confirmed);
        Assert.Equal(1, result.StatusBreakdown.Completed);
        Assert.Equal(0, result.StatusBreakdown.Cancelled);
    }

    [Fact]
    public async Task GetVendorEarnings_Ranks_TopListings_By_Gross_Revenue_Capped_At_Five()
    {
        var ctx = Build();
        var vendorUserId = Guid.NewGuid();
        var vendorProfileId = Guid.NewGuid();
        var day = new DateTime(2026, 3, 10);

        var bookings = new List<Booking>();
        for (int i = 0; i < 7; i++)
        {
            var listingId = Guid.NewGuid();
            // Higher index = higher revenue, so listing 6 should rank first.
            bookings.Add(MakeBooking(listingId, $"Listing {i}", BookingStatus.Completed, (i + 1) * 100, day));
        }
        // A Pending booking must never count toward top listings.
        bookings.Add(MakeBooking(Guid.NewGuid(), "Pending Listing", BookingStatus.Pending, 99999, day));

        ctx.BookingRepo.Setup(r => r.GetAllBookingsByVendorIdAsync(vendorUserId)).ReturnsAsync(bookings);
        ctx.RevenueReportService
            .Setup(s => s.GetGrossCommissionRefundBreakdownAsync(vendorProfileId, It.IsAny<RevenueReportRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GrossCommissionRefundBreakdownDto());
        ctx.RevenueReportService
            .Setup(s => s.GetReportAsync(vendorProfileId, It.IsAny<RevenueReportRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RevenuePeriodDto>());

        var request = new RevenueReportRequest { StartDate = day.AddDays(-1), EndDate = day.AddDays(1) };
        var result = await ctx.Service.GetVendorEarningsAsync(vendorUserId, vendorProfileId, request);

        Assert.Equal(5, result.TopListings.Count);
        Assert.Equal("Listing 6", result.TopListings[0].ListingTitle);
        Assert.Equal(700, result.TopListings[0].GrossRevenue);
        Assert.DoesNotContain(result.TopListings, l => l.ListingTitle == "Pending Listing");
    }

    [Theory]
    [InlineData(1000, 500, 100)]   // previous 500 -> current 1000 = +100%
    [InlineData(0, 0, 0)]          // no activity either period
    [InlineData(500, 0, 100)]      // previous 0, current > 0 -> treated as +100%
    public async Task GetVendorEarnings_Computes_RevenueChangePercent(decimal current, decimal previous, decimal expectedPercent)
    {
        var ctx = Build();
        var vendorUserId = Guid.NewGuid();
        var vendorProfileId = Guid.NewGuid();

        ctx.BookingRepo.Setup(r => r.GetAllBookingsByVendorIdAsync(vendorUserId)).ReturnsAsync(new List<Booking>());
        ctx.RevenueReportService
            .SetupSequence(s => s.GetGrossCommissionRefundBreakdownAsync(vendorProfileId, It.IsAny<RevenueReportRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GrossCommissionRefundBreakdownDto { NetEarnings = current })
            .ReturnsAsync(new GrossCommissionRefundBreakdownDto { NetEarnings = previous });
        ctx.RevenueReportService
            .Setup(s => s.GetReportAsync(vendorProfileId, It.IsAny<RevenueReportRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RevenuePeriodDto>());

        var request = new RevenueReportRequest { StartDate = new DateTime(2026, 3, 1), EndDate = new DateTime(2026, 3, 10) };
        var result = await ctx.Service.GetVendorEarningsAsync(vendorUserId, vendorProfileId, request);

        Assert.Equal(expectedPercent, result.RevenueChangePercent);
    }
}
