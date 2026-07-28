using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Payments;

public class RecordDisputeRequest
{
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal? DisputeFeeAmount { get; set; }
    public string? ExternalDisputeReference { get; set; }
}

public class ResolveDisputeRequest
{
    // Must be Won, Lost, or Withdrawn - never Opened.
    public PaymentDisputeStatus Outcome { get; set; }
}

public class PaymentDisputeDto
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal? DisputeFeeAmount { get; set; }
    public string? ExternalDisputeReference { get; set; }
    public PaymentDisputeStatus Status { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
