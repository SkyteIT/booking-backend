using Ube.Domain.Entities.Listings;

namespace Ube.Domain.Entities.Carts;

public class CartItem
{
    public Guid Id { get; set; }

    public Guid CartId { get; set; }
    public Cart Cart { get; set; } = default!;

    public Guid ListingId { get; set; }
    public Listing Listing { get; set; } = default!;

    public int Quantity { get; set; }

    public int GuestCount { get; set; } = 1;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
