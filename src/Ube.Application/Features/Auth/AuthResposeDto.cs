namespace Ube.Application.Features.Auth;
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime TokenExpiresAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    // Set only when a 2FA challenge is pending (Admin/Finance roles) -
    // Token/TokenExpiresAt/RefreshToken stay empty in that case, since
    // real tokens aren't issued until the challenge is verified.
    public bool RequiresTwoFactor { get; set; } = false;
    public bool RequiresEnrollment { get; set; } = false;
    public string? ChallengeToken { get; set; }
}