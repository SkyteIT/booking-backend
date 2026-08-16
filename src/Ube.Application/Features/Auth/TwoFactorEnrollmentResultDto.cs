namespace Ube.Application.Features.Auth;

// Backup codes are only ever included in this response, at enrollment
// confirmation time - never returned again afterward.
public class TwoFactorEnrollmentResultDto
{
    public AuthResponseDto Auth { get; set; } = new();
    public List<string> BackupCodes { get; set; } = new();
}
