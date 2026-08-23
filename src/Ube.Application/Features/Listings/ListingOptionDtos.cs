using Ube.Domain.Enums.Listings;

namespace Ube.Application.Features.Listings;

public class ListingOptionValueDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public decimal PriceModifier { get; set; }
    public decimal? PriceOverride { get; set; }
    public BookingConfirmationType? ConfirmationTypeOverride { get; set; }
    public bool RequiresSeatSelection { get; set; }
}

public class ListingOptionGroupDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<ListingOptionValueDto> Values { get; set; } = new();
}

public class AddListingOptionGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class UpdateListingOptionGroupRequest
{
    public string? Name { get; set; }
    public int? DisplayOrder { get; set; }
}

public class AddListingOptionValueRequest
{
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public decimal PriceModifier { get; set; }
    public decimal? PriceOverride { get; set; }
    public BookingConfirmationType? ConfirmationTypeOverride { get; set; }
    public bool RequiresSeatSelection { get; set; }
}

public class UpdateListingOptionValueRequest
{
    public string? Name { get; set; }
    public int? DisplayOrder { get; set; }
    public decimal? PriceModifier { get; set; }
    // Nullable-of-nullable isn't expressible in a plain request DTO, so a
    // separate flag says whether the caller actually means to change this
    // field (including clearing it back to "use category default"/"no override").
    public bool ClearConfirmationTypeOverride { get; set; }
    public BookingConfirmationType? ConfirmationTypeOverride { get; set; }
    public bool ClearPriceOverride { get; set; }
    public decimal? PriceOverride { get; set; }
    public bool? RequiresSeatSelection { get; set; }
}
