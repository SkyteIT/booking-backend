
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Enums.Bookings;

namespace Ube.Application.Features.Dashboard;

public class VendorDashboardService : IDashboardService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IListingRepository _listingRepository;

    public VendorDashboardService(
        IBookingRepository bookingRepository,
        IListingRepository listingRepository)
    {
        _bookingRepository = bookingRepository;
        _listingRepository = listingRepository;
    }

    public async Task<VendorDashboardDto> GetVendorDashboardAsync(Guid vendorId)
    {
        var bookings = await _bookingRepository.GetAllBookingsByVendorIdAsync(vendorId);

        var confirmedBookings = bookings
            .Where(b => b.Status == BookingStatus.Confirmed)
            .ToList();

        var totalRevenue = confirmedBookings.Sum(b => b.TotalAmount);
        var activeBookings = confirmedBookings.Count;
        var totalListings = (await _listingRepository.GetByVendorIdAsync(vendorId)).Count;

        return new VendorDashboardDto
        {
            TotalRevenue = totalRevenue,
            ActiveBookings = activeBookings,
            TotalListings = totalListings,
            AverageRating = 0
        };
    }

        public async Task<VendorBookingCountsDto> GetVendorBookingCountsAsync(Guid vendorId)
        {
            var bookings = await _bookingRepository.GetAllBookingsByVendorIdAsync(vendorId);

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