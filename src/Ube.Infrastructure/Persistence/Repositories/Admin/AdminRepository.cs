using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Admin.Dashboard;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Users;
using Ube.Domain.Enums.Bookings;

namespace Ube.Infrastructure.Persistence.Repositories.Admin;

public class AdminRepository : IAdminRepository
{
    private readonly ApplicationDbContext _context;

    public AdminRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // ── Users ─────────────────────────────────────────────────────────────────

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(u => u.Bookings)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users
            .Include(u => u.Bookings)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    // ── Bookings ──────────────────────────────────────────────────────────────

    public async Task<List<Booking>> GetAllBookingsAsync()
    {
        return await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Listing)
                .ThenInclude(l => l.Category)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Booking?> GetBookingByIdAsync(Guid bookingId)
    {
        return await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Listing)
                .ThenInclude(l => l.Category)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
    }

    public Task UpdateBookingAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        return Task.CompletedTask;
    }

    // ── Dashboard Stats ───────────────────────────────────────────────────────

    public async Task<int> GetTotalUsersCountAsync()
        => await _context.Users.CountAsync();

    public async Task<int> GetTotalBookingsCountAsync()
        => await _context.Bookings.CountAsync();

    public async Task<int> GetActiveBookingsCountAsync()
        => await _context.Bookings
            .CountAsync(b => b.Status == BookingStatus.Confirmed);

    public async Task<int> GetPendingBookingsCountAsync()
        => await _context.Bookings
            .CountAsync(b => b.Status == BookingStatus.Pending);

    public async Task<int> GetCancelledBookingsCountAsync()
        => await _context.Bookings
            .CountAsync(b => b.Status == BookingStatus.Cancelled);

    public async Task<decimal> GetTotalRevenueAsync()
        => await _context.Bookings
            .Where(b => b.Status == BookingStatus.Completed)
            .SumAsync(b => b.TotalAmount);

    public async Task<int> GetTotalListingsCountAsync()
        => await _context.Listings.CountAsync();

    public async Task<int> GetActiveListingsCountAsync()
        => await _context.Listings
            .CountAsync(l => l.IsActive);

    public async Task<int> GetTotalVendorsCountAsync()
        => await _context.Users
            .CountAsync(u => u.Role == UserRole.Vendor);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}