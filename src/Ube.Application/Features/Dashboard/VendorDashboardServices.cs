
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Payments;
using Ube.Application.Features.Reviews;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Enums.Bookings;

namespace Ube.Application.Features.Dashboard;

public class VendorDashboardService : IDashboardService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IListingRepository _listingRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IVendorRevenueReportService _revenueReportService;

    public VendorDashboardService(
        IBookingRepository bookingRepository,
        IListingRepository listingRepository,
        IReviewRepository reviewRepository,
        IVendorRevenueReportService revenueReportService)
    {
        _bookingRepository = bookingRepository;
        _listingRepository = listingRepository;
        _reviewRepository = reviewRepository;
        _revenueReportService = revenueReportService;
    }

    private async Task<List<Booking>> GetVendorBookingsAsync(Guid vendorId)
    {
        return await _bookingRepository.GetAllBookingsByVendorIdAsync(vendorId);
    }
    // Method to get vendor dashboard data
    public async Task<VendorDashboardDto> GetVendorDashboardAsync(Guid vendorId)
    {
        var bookings = await GetVendorBookingsAsync(vendorId);

        var confirmedBookings = bookings
            .Where(b => b.Status == BookingStatus.Confirmed)
            .ToList();

        var totalRevenue = confirmedBookings.Sum(b => b.TotalAmount);
        var activeBookings = confirmedBookings.Count;
        var totalListings = (await _listingRepository.GetByVendorIdAsync(vendorId)).Count;
        var (averageRating, _) = await _reviewRepository.GetRatingAsync(vendorId);

        return new VendorDashboardDto
        {
            TotalRevenue = totalRevenue,
            ActiveBookings = activeBookings,
            TotalListings = totalListings,
            AverageRating = Math.Round(averageRating, 2)
        };
    }
    // Method to get booking counts by status for the vendor
        public async Task<VendorBookingCountsDto> GetVendorBookingCountsAsync(Guid vendorId)
        {
            var bookings = await GetVendorBookingsAsync(vendorId);

            var counts = new VendorBookingCountsDto
            {
                Pending = bookings.Count(b => b.Status == BookingStatus.Pending),
                Confirmed = bookings.Count(b => b.Status == BookingStatus.Confirmed),
                Rejected = bookings.Count(b => b.Status == BookingStatus.Rejected),
                Cancelled = bookings.Count(b => b.Status == BookingStatus.Cancelled),
                Completed = bookings.Count(b => b.Status == BookingStatus.Completed),
                Total = bookings.Count()
            };

            return counts;
        }

    // Composes the vendor's "Earnings" tab: the gross/commission/refund
    // breakdown, revenue trend, period-over-period change, booking-status
   
    public async Task<VendorEarningsDto> GetVendorEarningsAsync(Guid vendorUserId, Guid vendorProfileId, RevenueReportRequest request, CancellationToken ct = default)
    {
        var start = (request.StartDate ?? DateTime.UtcNow.AddDays(-30)).Date;
        var end = request.EndDate ?? DateTime.UtcNow;

        var breakdown = await _revenueReportService.GetGrossCommissionRefundBreakdownAsync(vendorProfileId, request, ct);
        var revenueTrend = await _revenueReportService.GetReportAsync(vendorProfileId, request, ct);

        // Real period-over-period comparison, against the immediately
        // preceding period of equal length - not a client-side stub.
        var periodLength = end - start;
        var previousRequest = new RevenueReportRequest
        {
            StartDate = start - periodLength,
            EndDate = start,
            Granularity = request.Granularity
        };
        var previousBreakdown = await _revenueReportService.GetGrossCommissionRefundBreakdownAsync(vendorProfileId, previousRequest, ct);
        var revenueChangePercent = previousBreakdown.NetEarnings == 0
            ? (breakdown.NetEarnings == 0 ? 0 : 100)
            : (breakdown.NetEarnings - previousBreakdown.NetEarnings) / previousBreakdown.NetEarnings * 100;

        var bookingsInRange = (await GetVendorBookingsAsync(vendorUserId))
            .Where(b => b.CreatedAt >= start && b.CreatedAt <= end)
            .ToList();

        var statusBreakdown = new BookingStatusBreakdownDto
        {
            Pending = bookingsInRange.Count(b => b.Status == BookingStatus.Pending),
            Confirmed = bookingsInRange.Count(b => b.Status == BookingStatus.Confirmed),
            Rejected = bookingsInRange.Count(b => b.Status == BookingStatus.Rejected),
            Cancelled = bookingsInRange.Count(b => b.Status == BookingStatus.Cancelled),
            Completed = bookingsInRange.Count(b => b.Status == BookingStatus.Completed),
            Total = bookingsInRange.Count
        };

        // Top listings by gross booking value - matches how per-listing
        // breakdowns work on real marketplaces (net-of-commission stays
        // at the account level, in the breakdown/trend above).
        var topListings = bookingsInRange
            .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
            .GroupBy(b => b.ListingId)
            .Select(g => new ListingRevenueDto
            {
                ListingId = g.Key,
                ListingTitle = g.First().Listing.Title,
                GrossRevenue = g.Sum(b => b.TotalAmount),
                BookingCount = g.Count()
            })
            .OrderByDescending(l => l.GrossRevenue)
            .Take(5)
            .ToList();

        var totalBookings = bookingsInRange.Count;
        var averageBookingValue = totalBookings == 0 ? 0 : bookingsInRange.Sum(b => b.TotalAmount) / totalBookings;

        return new VendorEarningsDto
        {
            Breakdown = breakdown,
            RevenueTrend = revenueTrend,
            TotalBookings = totalBookings,
            AverageBookingValue = averageBookingValue,
            RevenueChangePercent = revenueChangePercent,
            StatusBreakdown = statusBreakdown,
            TopListings = topListings
        };
    }
}