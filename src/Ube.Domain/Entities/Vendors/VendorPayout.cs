
using Ube.Domain.Entities.Vendors;

namespace Ube.Domain.Entities.Vendors;
public class VendorPayout
{
    public Guid Id { get; set; }

    public Guid VendorProfileId { get; set; }

    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public VendorProfile VendorProfile { get; set; } = null!;
}