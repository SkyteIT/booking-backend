using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Bookings;
using Ube.Domain.Enums.Fraud;
using Ube.Domain.Enums.Payments;

namespace Ube.Domain.Entities.Fraud;

public class FraudFlag
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public FraudRuleType RuleTriggered { get; set; }
    public FraudFlagSeverity Severity { get; set; }
    public string Details { get; set; } = string.Empty;

    public FraudFlagStatus Status { get; set; } = FraudFlagStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }

    // Only set when Severity == Hold - what the booking's status/payment
    // collection method would have been had it not been held, computed once
    // by CheckoutService at creation time. Lets a Clear decision resolve the
    // booking without re-deriving Category booking-type rules a second time.
    public BookingStatus? ResolvedStatusOnClear { get; set; }
    public PaymentCollectionMethod? CollectionMethodOnClear { get; set; }
}
