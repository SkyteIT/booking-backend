using Ube.Domain.Enums.Listings;

namespace Ube.Application.Features.Listings;

public class ListingAddonDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public AddonPricingModel PricingModel { get; set; }
    public bool IsActive { get; set; }
}

public class CreateListingAddonRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public AddonPricingModel PricingModel { get; set; }
}

public class UpdateListingAddonRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public AddonPricingModel? PricingModel { get; set; }
    public bool? IsActive { get; set; }
}
