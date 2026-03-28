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
    public async Task<List<Booking>> GetBookingsByVendorIdAsync(
        Guid vendorId , BookingStatus? status , BookingSortBy? sortBy)
    {
        var query = _db.Bookings
            .Include(b => b.Customer)// get customer details for the booking
            .Include(b => b.Listing)// get Vendor Id through listing
            .Where(b => b.Listing.VendorProfileId == vendorId)
            .AsQueryable();
            if (status.HasValue)
            {
                //filter by status
                query = query.Where(b => b.Status ==status.Value);
            }
            query = sortBy switch
            {
                BookingSortBy.Oldest => query.OrderBy(b => b.CreatedAt), //oldest booking first
                BookingSortBy.StartDateAsc => query.OrderBy(b => b.StartDateTime), //near upcoming booking first
                BookingSortBy.StartDateDesc => query.OrderByDescending(b => b.StartDateTime), //far upcoming booking first
                _ => query.OrderByDescending(b => b.CreatedAt) //latest booking first   
            };
        return await query.ToListAsync();
    }

    public async Task<Booking?> GetBookingAsync(Guid BookingId, Guid vendorId){

        var query = _db.Bookings
            .Include(b => b.Listing)
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.Id == BookingId && b.Listing.VendorProfileId == vendorId);
        return await query;

    }

    public async Task UpdateAsync(Booking booking)
    {
        _db.Bookings.Update(booking);
        await _db.SaveChangesAsync();
    }
}