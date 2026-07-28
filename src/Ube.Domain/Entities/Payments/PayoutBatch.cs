using Ube.Domain.Enums.Payments;

namespace Ube.Domain.Entities.Payments;

public class PayoutBatch
{
    public Guid Id { get; set; }

    public Guid VendorProfileId { get; set; }

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalAmount { get; set; }

    public PayoutBatchStatus Status { get; set; } = PayoutBatchStatus.Pending;

    public Guid? SettledByUserId { get; set; }
    public DateTime? SettledAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
