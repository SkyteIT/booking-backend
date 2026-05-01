using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Users;

namespace Ube.Domain.Entities.Reviews;

public class Review
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = default!;

    public Guid ListingId { get; set; }
    public Listing Listing { get; set; } = default!;

    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = default!;
    public Guid VendorId { get; set; }
    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}