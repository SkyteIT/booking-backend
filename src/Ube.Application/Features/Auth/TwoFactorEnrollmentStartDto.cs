namespace Ube.Application.Features.Auth;
public class TwoFactorEnrollmentStartDto
{
    public string Secret { get; set; } = string.Empty;
    public string OtpAuthUri { get; set; } = string.Empty;
}
