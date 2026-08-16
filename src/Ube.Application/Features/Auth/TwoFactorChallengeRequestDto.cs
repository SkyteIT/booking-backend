namespace Ube.Application.Features.Auth;
public class TwoFactorChallengeRequestDto
{
    public string ChallengeToken { get; set; } = string.Empty;
}
