namespace Ube.Application.Common.Models;

public class BookingCompletionOptions
{
    public bool Enabled { get; set; } = true;
    public int RunIntervalHours { get; set; } = 1;
}
