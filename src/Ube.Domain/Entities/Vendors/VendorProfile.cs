namespace Ube.Domain.Entities.Vendors;

public class VendorProfile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string BusinessType { get; set; } = string.Empty;

    public string BusinessDescription { get; set; } = string.Empty;

    public string Description
    {
        get => BusinessDescription;
        set => BusinessDescription = value;
    }

    public string ContactNumber { get; set; } = string.Empty;


    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}