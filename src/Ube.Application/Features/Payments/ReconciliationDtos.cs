namespace Ube.Application.Features.Payments;

public class ReconciliationMismatchDto
{
    public Guid PaymentId { get; set; }
    public string ExpectedStatus { get; set; } = string.Empty;
    public string GatewayReportedStatus { get; set; } = string.Empty;
}

public class ReconciliationReportDto
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int PaymentsChecked { get; set; }
    public IReadOnlyList<ReconciliationMismatchDto> Mismatches { get; set; } = [];
    public DateTime RanAt { get; set; }
}
