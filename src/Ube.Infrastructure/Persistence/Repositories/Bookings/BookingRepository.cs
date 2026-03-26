using Microsoft.EntityFrameworkCore;
using Ube.Application.common.Interfaces.Persistence;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Enums.Bookings;

namespace Ube.Infrastructure.Persistence.Repositories.Bookings;

public class BookingRepository : IBookingRepository
{
    private readonly ApplicationDbContext _db;

    public BookingRepository(ApplicationDbContext context)
    {
        _db = context;
    }

//
    public async Task<Booking?> GetByIdAsync(Guid bookingId)
    {
        return await _db.Bookings
            .Include(b => b.Listing)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
    }
    public async Task<int> GetNextBookingSequenceAsync()
    {
        var result = await _db
            .Database
            .SqlQueryRaw<int>("SELECT NEXT VALUE FOR BookingNumbers")
            .FirstAsync();

        return result;
    }
    public async Task<List<Booking>> GetBookingsByVendorIdAsync(Guid vendorId , BookingStatus? status )
    {
        var query = _db.Bookings
            .Include(b => b.Listing)
            .Where(b => b.Listing.VendorProfileId == vendorId);
            if (status.HasValue)
        {
            //filter by status
            query = query.Where(b => b.Status ==status.Value);

        }
        return await query
            .OrderByDescending(b => b.CreatedAt) 
            .ToListAsync();
    }

    public async Task UpdateAsync(Booking booking)
    {
        _db.Bookings.Update(booking);
        await _db.SaveChangesAsync();
    }
}