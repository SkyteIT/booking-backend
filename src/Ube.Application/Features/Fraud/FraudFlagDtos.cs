using Ube.Domain.Enums.Fraud;

namespace Ube.Application.Features.Fraud;

public class FraudFlagDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public FraudRuleType RuleTriggered { get; set; }
    public FraudFlagSeverity Severity { get; set; }
    public string Details { get; set; } = string.Empty;
    public FraudFlagStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
}

// Admin queue row - joined with Booking/Customer for display, same shape
// as PaymentDisputeService's AdminDisputeDto.
public class AdminFraudFlagDto : FraudFlagDto
{
    public string BookingNumber { get; set; } = string.Empty;
    public string ListingTitle { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
}

public enum FraudReviewDecision
{
    Clear = 1,
    ConfirmFraud = 2
}

public class ReviewFraudFlagRequest
{
    public FraudReviewDecision Decision { get; set; }
    public string? Notes { get; set; }
}
