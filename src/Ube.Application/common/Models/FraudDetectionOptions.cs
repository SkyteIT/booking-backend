namespace Ube.Application.Common.Models;

public class FraudDetectionOptions
{
    public bool Enabled { get; set; } = true;

    // NewAccountHighValue (Hold)
    public int NewAccountHours { get; set; } = 24;
    public decimal HighValueThreshold { get; set; } = 50000;

    // BookingVelocity (FlagOnly)
    public int MaxBookingsPerWindow { get; set; } = 5;
    public int VelocityWindowMinutes { get; set; } = 60;

    // RepeatedCancellations (FlagOnly)
    public int MaxCancellationsPerWindow { get; set; } = 3;
    public int CancellationWindowDays { get; set; } = 7;
}
