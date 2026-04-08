
using Ube.Domain.Entities.Listings;

namespace Ube.Application.Features.Availablity;

public async Task<List<CalanderDayDto>> GetCalanderAsync(Guid listingId, int month, int year)
{
    if (month < 1 || month > 12)
        throw new ArgumentException("Month must be between 1 and 12.");
    if (year < 1)
        throw new ArgumentException("Year must be a positive integer.");
    var capacity = await _listingRepository.GetCapacityAsync(listingId);
}