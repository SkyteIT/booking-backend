using Ube.Domain.Entities.Common;
using Ube.Domain.Enums;
using Ube.Domain.Entities.Listings;

namespace Ube.Domain.Entities.Content;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? BannerImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool RequiresAdminApproval { get; set; }
    public bool IsFeatured { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;

    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
}
