namespace Ube.Domain.Entities.Users;

public class UserLocalizationSettings
{
    public Guid Id {get ; set;}
    public Guid UserId { get; set; }
    public string Language { get ; set;} = "en";
    public string TimeZone { get ; set;} = "UTC";
    public string Currency { get ; set;} = "LKR";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


}