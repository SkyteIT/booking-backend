namespace Ube.Domain.Enums.Payments;

public enum LedgerEntryType
{
    Charge = 1,
    Commission = 2,
    PlatformFee = 3,
    Refund = 4,
    AdvancePayout = 5,
    Settlement = 6,
    Clawback = 7,
    InvoicePayment = 8,

    // Provisional debit the moment a bank/card dispute is recorded -
    // the bank pulls the money back immediately, before any outcome is known.
    Dispute = 9,
    // The flat fee the gateway charges the platform just for handling a
    // dispute, regardless of who wins it.
    DisputeFee = 10,
    // Reverses a Dispute entry when the platform wins or the dispute is withdrawn.
    DisputeReversal = 11
}
