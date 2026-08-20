namespace Ube.Application.Features.Auth;

public class GoogleLoginRequest
{
    public string IdToken { get; set; } = string.Empty;
    public string? DeviceToken { get; set; }
}
