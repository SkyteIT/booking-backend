using Microsoft.EntityFrameworkCore;
using Ube.Application.common.Interfaces.Persistence;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Enums.Bookings;
using Ube.Application.Features.Bookings.Requests;
using Ube.Application.common.Models.pagination;


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
    public async Task<PagedResult<Booking>> GetBookingsByVendorIdAsync(
        Guid vendorId , BookingsRequest request)
    {
        var query = _db.Bookings
            .Include(b => b.Customer)// get customer details for the booking
            .Include(b => b.Listing)// get Vendor Id through listing
            .Where(b => b.Listing.VendorProfileId == vendorId)
            .AsQueryable();
            if (request.Status.HasValue)
            {
                //filter by status
                query = query.Where(b => b.Status ==request.Status.Value);
            }
            query = request.SortBy switch
            {
                BookingSortBy.Oldest => query.OrderBy(b => b.CreatedAt), //oldest booking first
                BookingSortBy.StartDateAsc => query.OrderBy(b => b.StartDateTime), //near upcoming booking first
                BookingSortBy.StartDateDesc => query.OrderByDescending(b => b.StartDateTime), //far upcoming booking first
                _ => query.OrderByDescending(b => b.CreatedAt) //latest booking first   
            };
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();
        return new PagedResult<Booking>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };

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