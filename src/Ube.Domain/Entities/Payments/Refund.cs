using Ube.Domain.Enums.Payments;

namespace Ube.Domain.Entities.Payments;

public class Refund
{
    public Guid Id { get; set; }

    public Guid PaymentId { get; set; }

    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;

    public RefundStatus Status { get; set; } = RefundStatus.Requested;
    public decimal PolicyTierApplied { get; set; }

    public Guid RequestedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }

    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
