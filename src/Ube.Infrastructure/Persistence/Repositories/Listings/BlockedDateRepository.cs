using Microsoft.EntityFrameworkCore;

using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Repositories.Listings;

public class BlockedDateRepository :IBlockedDateRepository  
{
    private readonly ApplicationDbContext _db;
    public BlockedDateRepository(ApplicationDbContext db)
    {
        _db = db;
    }
    //get block dates for a listing in a date range
    public async Task<List<BlockedDate>> GetByListingAndDateRangeAsync(Guid listingId, DateTime startDate, DateTime endDate)
    {
        return await _db.BlockedDates
            .Where(bd => bd.ListingId == listingId && 
                    bd.Date >= startDate.Date && 
                    bd.Date <= endDate.Date)
            .ToListAsync();
    }
    // get block dates for a listing for specific dates (used for availability check)
     public async Task<List<BlockedDate>> GetByListingAndDatesAsync(
            Guid listingId,
            List<DateTime> dates)
        {
            return await _db.BlockedDates
                .Where(x => x.ListingId == listingId &&
                            dates.Contains(x.Date))
                .ToListAsync();
        }

    // Add multiple blocked dates
    public async Task AddRangeAsync(List<BlockedDate> blockedDates)
    {
        await _db.BlockedDates.AddRangeAsync(blockedDates);
        await _db.SaveChangesAsync();
    }

    //Remove multiple blocked dates
    public async Task RemoveRangeAsync(List<BlockedDate> blockedDates)
    {
        _db.BlockedDates.RemoveRange(blockedDates);
        await _db.SaveChangesAsync();
    }

}
