namespace Ube.Application.Features.Notifications.Email;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string token);
    Task SendEmailChangeVerificationEmailAsync(string newEmail, string token);
    Task SendPasswordResetEmailAsync(string email, string token);
    Task SendWelcomeEmailAsync(string email, string firstName);
    Task SendVendorApplicationSubmittedEmailAsync(string email, string firstName, string businessName);
    Task SendVendorApplicationApprovedEmailAsync(string email, string firstName, string businessName);
    Task SendVendorApplicationRejectedEmailAsync(string email, string firstName, string businessName, string? rejectionReason);
    Task SendEmailAsync(string to, string subject, string htmlBody);
}
