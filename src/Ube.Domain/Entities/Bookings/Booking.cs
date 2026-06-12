using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Bookings;

namespace Ube.Domain.Entities.Bookings;

public class Booking
{
    public Guid Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;

    public Guid ListingId { get; set; }
    public Listing Listing { get; set; } = default!;

    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = default!;

    public DateTime StartDateTime { get; set; }

    public DateTime EndDateTime { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "LKR";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Optimistic concurrency — prevents two simultaneous status updates from both succeeding
    public byte[] RowVersion { get; set; } = [];
}