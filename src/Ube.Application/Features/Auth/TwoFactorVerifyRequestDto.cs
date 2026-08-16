namespace Ube.Application.Features.Auth;
public class TwoFactorVerifyRequestDto
{
    public string ChallengeToken { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
