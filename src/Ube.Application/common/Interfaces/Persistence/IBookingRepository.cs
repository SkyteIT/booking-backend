using Ube.Domain.Entities.Bookings;
using Ube.Application.Features.Bookings;
using Ube.Application.Common.Models.Pagination;

namespace Ube.Application.Common.Interfaces.Persistence;

public interface IBookingRepository
{
    Task<Booking ?> GetByIdAsync(Guid BookingId);
    Task UpdateAsync(Booking booking);
    Task<PagedResult<Booking>> GetBookingsByVendorIdAsync(Guid vendorId, BookingsRequest request);
    Task<List<Booking>> GetAllBookingsByVendorIdAsync(Guid vendorId);
    Task<Booking?>GetBookingAsync(Guid BookingId , Guid vendorId);
    Task<int> GetNextBookingSequenceAsync();

    Task <List<Booking>> GetBookingsByListingAndDateRangeAsync(Guid listingId , DateTime startDate , DateTime endDate);

    Task<List<Booking>> GetBookingsByListingUnitAndDateRangeAsync(Guid listingUnitId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task AddAsync(Booking booking, CancellationToken ct = default);
    Task<PagedResult<Booking>> GetBookingsByCustomerIdAsync(Guid customerId, BookingsRequest request, CancellationToken ct = default);
    Task<List<Booking>> GetBookingsPastEndDateAsync(DateTime asOf, CancellationToken ct = default);
    Task<Dictionary<Guid, string>> GetBookingNumbersByIdsAsync(IEnumerable<Guid> bookingIds, CancellationToken ct = default);

    // Fraud detection support - see Features/Fraud/FraudDetectionService.cs.
    Task<bool> HasOverlappingBookingForCustomerAsync(Guid customerId, Guid listingId, Guid? listingUnitId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<int> CountByCustomerSinceAsync(Guid customerId, DateTime since, CancellationToken ct = default);
    Task<int> CountCancelledByCustomerSinceAsync(Guid customerId, DateTime since, CancellationToken ct = default);
}