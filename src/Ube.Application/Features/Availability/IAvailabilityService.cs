

namespace Ube.Application.Features.Availability;

public interface IAvailabilityService
{
    Task<List<CalanderDayDto>> GetCalanderAsync(Guid listingId, Guid vendorId, int month, int year);
    Task BlockdatesAsync(Guid listingId, Guid vendorId, List<DateTime> dates);
    Task UnBlockdatesAsync(Guid listingId, Guid vendorId, List<DateTime> dates);
}

