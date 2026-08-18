using Ube.Domain.Enums;
namespace Ube.Application.Features.Availability;

public class CalanderDayDto
{
    public DateTime Date { get; set;}
    //status of date - available, booked, blocked
    public AvailabilityStatus Status { get; set;}
    //how many units are available
    public int AvailableCount {get;set;}
    public int BookingCount { get; set;}
    public bool IsBlocked { get; set;}
    
}