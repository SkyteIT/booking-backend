using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Bookings;

namespace Ube.Domain.Entities.Bookings;

public class Booking
{
    public Guid Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;

    public Guid ListingId { get; set; }
    public Listing Listing { get; set; } = null!;

    // Which specific bookable unit (room type, seat, fleet vehicle, time
    // slot) this booking is for - null for listings with no defined
    // units, in which case the booking is against the listing as a whole.
    public Guid? ListingUnitId { get; set; }
    public ListingUnit? ListingUnit { get; set; }

    // Comma-separated ListingOptionValue ids selected at checkout (e.g.
    // Room Type: Deluxe, Stay Type: Overnight) - a simple denormalized
    // record for the receipt/admin view. Nothing currently needs to query
    // bookings by a specific option value, so this stays a plain column
    // rather than a join table.
    public string? SelectedOptionValueIds { get; set; }

    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "LKR";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Set only by the NewAccountHighValue fraud rule at checkout - while
    // true, the booking is forced Pending and payment capture is deferred
    // until an admin clears or rejects the FraudFlag that caused the hold.
    public bool IsHeldForFraudReview { get; set; }

    // Optimistic concurrency — prevents two simultaneous status updates from both succeeding
    public byte[] RowVersion { get; set; } = [];
}
