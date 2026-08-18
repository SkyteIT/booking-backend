
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Enums.Bookings;

namespace Ube.Application.Features.Dashboard;

public class VendorDashboardService : IDashboardService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IListingRepository _listingRepository;
    private readonly IReviewRepository _reviewRepository;

    public VendorDashboardService(
        IBookingRepository bookingRepository,
        IListingRepository listingRepository,
        IReviewRepository reviewRepository)
    {
        _bookingRepository = bookingRepository;
        _listingRepository = listingRepository;
        _reviewRepository = reviewRepository;
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
}