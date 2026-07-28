namespace Ube.Application.Features.Listings;

public class ListingCustomFieldValueInputDto
{
    public Guid CategoryCustomFieldId { get; set; }

    // For a multi-select Dropdown this is a comma separated list of the
    // chosen options; for Text/Number/Date/Checkbox it's the single value.
    public string Value { get; set; } = string.Empty;
}
