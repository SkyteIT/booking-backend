using Ube.Domain.Enums.Listings;

namespace Ube.Application.Features.Content.Category;

public class CategoryCustomFieldInputDto
{
    // Null = new field. Set to an existing field's Id to update it in place
    // (preserving any listing values already recorded against it) instead
    // of it being treated as a brand new field.
    public Guid? Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public CustomFieldType FieldType { get; set; }
    public bool Required { get; set; }
    public int DisplayOrder { get; set; }

    // Required (non-empty) when FieldType == Dropdown, ignored otherwise.
    public List<string>? Options { get; set; }
}
