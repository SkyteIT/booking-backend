using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Payments;

public class ComputePayoutBatchRequest
{
    public Guid VendorProfileId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

public class PayoutBatchDto
{
    public Guid Id { get; set; }
    public Guid VendorProfileId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalAmount { get; set; }
    public PayoutBatchStatus Status { get; set; }
    public DateTime? SettledAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
