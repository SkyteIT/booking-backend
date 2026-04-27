namespace Ube.Domain.Entities.Listings;


using Ube.Domain.Entities.Vendors;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Reviews;
using Ube.Domain.Entities.Carts;



public class Listing
{
    public Guid Id { get; set; }

    public Guid VendorProfileId { get; set; }

    public Guid CategoryId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Currency { get; set; } = "LKR";

    public string? Location { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }




    public VendorProfile VendorProfile { get; set; } = default!;
    public Category Category { get; set; } = default!;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}