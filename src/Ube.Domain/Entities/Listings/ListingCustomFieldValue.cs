namespace Ube.Domain.Entities.Listings;

// A listing's answer to one of its category's custom fields. Value is a raw
// string; for a multi-select Dropdown (e.g. Amenities) it's a comma
// separated list of the chosen options, same convention as Value storage
// elsewhere in this codebase.
public class ListingCustomFieldValue
{
    public Guid Id { get; set; }

    public Guid ListingId { get; set; }
    public Listing Listing { get; set; } = null!;

    public Guid CategoryCustomFieldId { get; set; }
    public CategoryCustomField CategoryCustomField { get; set; } = null!;

    public string Value { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
