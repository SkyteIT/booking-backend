using Ube.Domain.Entities.Listings;

namespace Ube.Application.Common.Interfaces.Persistence;

public interface IBlockedDateRepository
{
    //get all blocked dates for a listing(use for calander view)
    Task<List<BlockedDate>> GetByListingAndDateRangeAsync(Guid listingId, DateTime startDate, DateTime endDate);

    //get blocked dates  for a listing(used in block and unblock operations)
    Task<List<BlockedDate>> GetByListingAndDatesAsync(Guid listingId, List<DateTime> dates);
    //add multiple blocked dates
    Task AddRangeAsync(List<BlockedDate> blockedDates);

    //remove multiple blocked dates
    Task RemoveRangeAsync(List<BlockedDate> blockedDates);
}