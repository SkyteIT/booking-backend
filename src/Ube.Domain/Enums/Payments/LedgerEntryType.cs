namespace Ube.Domain.Enums.Payments;

public enum LedgerEntryType
{
    Charge = 1,
    Commission = 2,
    PlatformFee = 3,
    Refund = 4,
    AdvancePayout = 5,
    Settlement = 6,
    Clawback = 7
}
