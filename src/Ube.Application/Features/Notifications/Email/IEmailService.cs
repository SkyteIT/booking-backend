namespace Ube.Application.Features.Notifications.Email;
public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string token);
}