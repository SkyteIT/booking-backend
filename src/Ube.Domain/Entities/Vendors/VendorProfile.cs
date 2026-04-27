namespace Ube.Domain.Entities.Vendors;

using Ube.Domain.Entities.Users;
using Ube.Domain.Entities.Listings;


public class VendorProfile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string BusinessType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ContactNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }



    public User User { get; set; } = default!;
    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
}