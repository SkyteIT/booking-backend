namespace Ube.Application.Common.Models;

public class PaymentSchedulerOptions
{
    public bool Enabled { get; set; } = true;
    public int RunIntervalHours { get; set; } = 24;
    public int ReconciliationLookbackHours { get; set; } = 24;
}
