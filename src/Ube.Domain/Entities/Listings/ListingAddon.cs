using Ube.Domain.Enums.Listings;

namespace Ube.Domain.Entities.Listings;

public class ListingAddon
{
    public Guid Id { get; set; }

    public Guid ListingId { get; set; }
    public Listing Listing { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public decimal Price { get; set; }
    
    public AddonPricingModel PricingModel { get; set; }
    
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
