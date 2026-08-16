namespace Ube.Application.Features.Listings;

public class ListingCustomFieldValueDto
{
    public Guid CategoryCustomFieldId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
