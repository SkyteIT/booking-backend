using System.ComponentModel.DataAnnotations;

namespace Ube.Application.Features.Notifications;

public class SubscribePushDto
{
    [Required]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public string P256dh { get; set; } = string.Empty;

    [Required]
    public string Auth { get; set; } = string.Empty;
}

public class UnsubscribePushDto
{
    [Required]
    public string Endpoint { get; set; } = string.Empty;
}
