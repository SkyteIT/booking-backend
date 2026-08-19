namespace Ube.Application.Features.Notifications.Email;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string token);
    Task SendWelcomeEmailAsync(string email, string firstName);
    Task SendVendorApplicationSubmittedEmailAsync(string email, string firstName, string businessName);
    Task SendEmailAsync(string to, string subject, string htmlBody);
}
