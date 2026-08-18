using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Enums.Bookings;
using Ube.Application.Features.Bookings;
using Ube.Application.Common.Models.Pagination;

namespace Ube.Infrastructure.Persistence.Repositories.Bookings;

public class BookingRepository : IBookingRepository
{
    private readonly ApplicationDbContext _db;
    //inject  DI for database context
    public BookingRepository(ApplicationDbContext context)
    {
        _db = context;
    }

    //get booking by id with listing and customer details
    public async Task<Booking?> GetByIdAsync(Guid bookingId)
    {
        return await _db.Bookings
            .Include(b => b.Listing)
                .ThenInclude(l => l.VendorProfile) // include vendor profile through listing
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
    }
    // Get next booking sequence number for generating booking reference number.
    // Must materialize with ToListAsync, not FirstAsync/SingleAsync - EF
    // composes those as a wrapping subquery, and SQL Server rejects
    // "NEXT VALUE FOR" inside a subquery ("not allowed in ... sub-queries").
    public async Task<int> GetNextBookingSequenceAsync()
    {
        var results = await _db
            .Database
            .SqlQueryRaw<int>("SELECT NEXT VALUE FOR BookingNumbers")
            .ToListAsync();

        return results[0];
    }
    // get ookings for booking management page for vendors with pagination and filtering
    public async Task<PagedResult<Booking>> GetBookingsByVendorIdAsync(
        Guid vendorId , BookingsRequest request)
    {
        var query = _db.Bookings
            .Include(b => b.Customer)// get customer details for the booking
            .Include(b => b.Listing)
                .ThenInclude(l => l.VendorProfile) // include vendor profile through listing
            .Where(b => b.Listing.VendorProfile.UserId == vendorId)// filter by vendor id
            .AsQueryable();
            if (request.Status.HasValue)
            {
                //filter by status
                query = query.Where(b => b.Status ==request.Status.Value);
            }
            if (request.StartDate.HasValue)
            {
                query = query.Where(b => b.StartDateTime.Date >= request.StartDate.Value.Date);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(b => b.EndDateTime.Date <= request.EndDate.Value.Date);
            }
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(b =>
                    b.BookingNumber.ToLower().Contains(search) ||
                    b.Listing.Title.ToLower().Contains(search) ||
                    (b.Customer.FirstName + " " + b.Customer.LastName).ToLower().Contains(search));
            }
            query = request.SortOptions switch
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
    //use for analytics and reporting for vendors to get all bookings without pagination
    public async Task<List<Booking>> GetAllBookingsByVendorIdAsync(Guid vendorId)
    {
        return await _db.Bookings
            .Include(b => b.Listing)
            .Where(b => b.Listing.VendorProfile.UserId == vendorId)
            .ToListAsync();
    }

    // Get a specific booking for a vendor (used for booking details page)
    public async Task<Booking?> GetBookingAsync(Guid BookingId, Guid vendorId){

        var query = _db.Bookings
            .Include(b => b.Listing)
                .ThenInclude(l => l.VendorProfile) // include vendor profile through listing
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.Id == BookingId && b.Listing.VendorProfile.UserId == vendorId);//ensure booking belongs to vendor
        return await query;

    }

    public async Task UpdateAsync(Booking booking)
    {
        _db.Bookings.Update(booking);
        await _db.SaveChangesAsync();
    }

    // Get bookings for a listing in a date range(use for availability check)
    public async Task<List<Booking>> GetBookingsByListingAndDateRangeAsync(
        Guid listingId,
        DateTime startDate,
        DateTime endDate)
    {
        return await _db.Bookings
            .Where( b => b.ListingId == listingId &&
                    b.StartDateTime.Date <= endDate.Date &&
                    b.EndDateTime.Date >= startDate.Date &&
                    (b.Status == BookingStatus.Confirmed || 
                        (b.Status == BookingStatus.Pending && b.CreatedAt >= DateTime.UtcNow.AddHours(-1))) // consider pending bookings created within last 1 hour as they might still be confirmed
                    )
            .ToListAsync();
    }

    // Same soft-hold rule as GetBookingsByListingAndDateRangeAsync, scoped
    // to a specific bookable unit (room type/seat/time slot) instead of
    // the whole listing.
    public async Task<List<Booking>> GetBookingsByListingUnitAndDateRangeAsync(
        Guid listingUnitId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _db.Bookings
            .Where(b => b.ListingUnitId == listingUnitId &&
                    b.StartDateTime.Date <= endDate.Date &&
                    b.EndDateTime.Date >= startDate.Date &&
                    (b.Status == BookingStatus.Confirmed ||
                        (b.Status == BookingStatus.Pending && b.CreatedAt >= DateTime.UtcNow.AddHours(-1)))
                    )
            .ToListAsync(ct);
    }

    public async Task AddAsync(Booking booking, CancellationToken ct = default)
    {
        await _db.Bookings.AddAsync(booking, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<Booking>> GetBookingsByCustomerIdAsync(Guid customerId, BookingsRequest request, CancellationToken ct = default)
    {
        var query = _db.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Listing)
                .ThenInclude(l => l.VendorProfile)
            .Where(b => b.CustomerId == customerId)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(b => b.Status == request.Status.Value);
        if (request.StartDate.HasValue)
            query = query.Where(b => b.StartDateTime.Date >= request.StartDate.Value.Date);
        if (request.EndDate.HasValue)
            query = query.Where(b => b.EndDateTime.Date <= request.EndDate.Value.Date);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(b =>
                b.BookingNumber.ToLower().Contains(search) ||
                b.Listing.Title.ToLower().Contains(search));
        }
        query = request.SortOptions switch
        {
            BookingSortBy.Oldest => query.OrderBy(b => b.CreatedAt),
            BookingSortBy.StartDateAsc => query.OrderBy(b => b.StartDateTime),
            BookingSortBy.StartDateDesc => query.OrderByDescending(b => b.StartDateTime),
            _ => query.OrderByDescending(b => b.CreatedAt)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Booking>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
    }

    // Confirmed bookings whose service period has already ended - the
    // candidate set for the automatic "mark Completed" sweep.
    public async Task<List<Booking>> GetBookingsPastEndDateAsync(DateTime asOf, CancellationToken ct = default)
    {
        return await _db.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed && b.EndDateTime < asOf)
            .ToListAsync(ct);
    }

    // Bulk-resolve the human-readable BookingNumber ("BKG-000001") for a
    // set of booking ids - one grouped query, not one lookup per row, for
    // callers (e.g. the ledger) that only have raw booking ids to display.
    public async Task<Dictionary<Guid, string>> GetBookingNumbersByIdsAsync(IEnumerable<Guid> bookingIds, CancellationToken ct = default)
    {
        var ids = bookingIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<Guid, string>();

        return await _db.Bookings
            .Where(b => ids.Contains(b.Id))
            .Select(b => new { b.Id, b.BookingNumber })
            .ToDictionaryAsync(x => x.Id, x => x.BookingNumber, ct);
    }

    // Duplicate-booking guard - same customer, same listing/unit, dates
    // overlap, and the prior booking is still live (Pending/Confirmed).
    // A Cancelled/Rejected/Completed prior booking never blocks a new one.
    public async Task<bool> HasOverlappingBookingForCustomerAsync(
        Guid customerId, Guid listingId, Guid? listingUnitId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _db.Bookings.AnyAsync(b =>
            b.CustomerId == customerId &&
            b.ListingId == listingId &&
            b.ListingUnitId == listingUnitId &&
            (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed) &&
            b.StartDateTime.Date <= endDate.Date &&
            b.EndDateTime.Date >= startDate.Date,
            ct);
    }

    // Booking-velocity fraud signal - how many bookings this customer has
    // created since a given point in time, regardless of status.
    public async Task<int> CountByCustomerSinceAsync(Guid customerId, DateTime since, CancellationToken ct = default)
    {
        return await _db.Bookings.CountAsync(b => b.CustomerId == customerId && b.CreatedAt >= since, ct);
    }

    // Repeated-cancellation fraud signal.
    public async Task<int> CountCancelledByCustomerSinceAsync(Guid customerId, DateTime since, CancellationToken ct = default)
    {
        return await _db.Bookings.CountAsync(b =>
            b.CustomerId == customerId && b.Status == BookingStatus.Cancelled && b.CreatedAt >= since, ct);
    }
}