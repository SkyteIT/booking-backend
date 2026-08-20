namespace Ube.Application.Features.Notifications.Email;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string token);
    Task SendEmailChangeVerificationEmailAsync(string newEmail, string token);
    Task SendPasswordResetEmailAsync(string email, string token);
    Task SendEmailAsync(string to, string subject, string htmlBody);
}
