namespace Ube.Application.Features.Auth;

public class RegistrationResponseDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool RequiresEmailVerification { get; set; } = true;
    public bool VerificationEmailSent { get; set; }
    public string Message { get; set; } = string.Empty;
}
