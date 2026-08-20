namespace Ube.Application.Features.Auth;
public class TwoFactorVerifyRequestDto
{
    public string ChallengeToken { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    // "Remember this device for 7 days" - issues a TrustedDevice token
    // (returned as AuthResponseDto.DeviceToken) that skips future 2FA
    // challenges on this browser until it expires.
    public bool RememberDevice { get; set; } = false;
}
