namespace Ube.Domain.Enums.Payments;

public enum PayoutBatchStatus
{
    Pending = 1,
    Processing = 2,
    Settled = 3,

    // Included in a generated payout export file - locked from being
    // picked up by a second export so the same batch can never be paid
    // out twice.
    Exported = 4
}
