namespace Ube.Application.Features.Availablity;

public class CalanderDayDto
{
    public DateTime Date { get; set;}
    //status of date - available, booked, blocked
    public string  IsAvailable { get; set;} = string.Empty;
    //how many units are available
    public int AvailableCount {get;set;}
}