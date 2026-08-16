using Ube.Domain.Enums.Listings;

namespace Ube.Application.Features.Content.Category;

public class CategoryCustomFieldDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public CustomFieldType FieldType { get; set; }
    public bool Required { get; set; }
    public int DisplayOrder { get; set; }
    public List<string>? Options { get; set; }
}
