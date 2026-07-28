using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Payments;

public class ComputeVendorInvoiceRequest
{
    public Guid VendorProfileId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    // Days after PeriodEnd the vendor has to pay before it's flagged
    // Overdue. Admin-supplied per invoice rather than hardcoded, since
    // grace periods are a business/finance decision, not a constant.
    public int GracePeriodDays { get; set; } = 14;
}

public class VendorInvoiceDto
{
    public Guid Id { get; set; }
    public Guid VendorProfileId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal AmountOwed { get; set; }
    public DateTime DueDate { get; set; }
    public VendorInvoiceStatus Status { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
