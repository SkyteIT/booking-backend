namespace Ube.Domain.Enums.Payments;

public enum PaymentStatus
{
    Pending = 1,
    Authorized = 2,
    Captured = 3,
    Failed = 4,
    Refunded = 5,
    PartiallyRefunded = 6,

    // A bank/card dispute was lost - the money is gone for good, distinct
    // from a Refunded payment (we chose to refund) since this was taken
    // from us by the bank.
    ChargedBack = 7
}
