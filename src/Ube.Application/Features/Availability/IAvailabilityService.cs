

namespace Ube.Application.Features.Availability;

public interface IAvailabilityService
{
    Task<List<CalanderDayDto>> GetCalanderAsync(Guid listingId, int month, int year);
    Task BlockdatesAsync(Guid listingId, List<DateTime> dates);
    Task UnBlockdatesAsync(Guid listingId, List<DateTime> dates);
}

