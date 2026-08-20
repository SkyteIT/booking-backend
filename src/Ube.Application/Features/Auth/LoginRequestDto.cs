namespace Ube.Application.Features.Auth;
public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // Sent back from a previous "remember this device" 2FA verification -
    // if it matches an unexpired TrustedDevice, the mandatory 2FA challenge
    // (Admin/Finance/SuperAdmin) is skipped for this login.
    public string? DeviceToken { get; set; }
}