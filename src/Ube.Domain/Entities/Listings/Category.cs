namespace Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Listings;

public class Category
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }


    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
}