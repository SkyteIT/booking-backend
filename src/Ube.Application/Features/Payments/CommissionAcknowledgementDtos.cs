namespace Ube.Application.Features.Payments;

public class AcknowledgeCommissionRateRequest
{
    public Guid CategoryId { get; set; }
}

public class CommissionAcknowledgementDto
{
    public Guid Id { get; set; }
    public Guid VendorProfileId { get; set; }
    public decimal CommissionPercentShown { get; set; }
    public Guid? SourceOverrideId { get; set; }
    public DateTime AcknowledgedAt { get; set; }
}
