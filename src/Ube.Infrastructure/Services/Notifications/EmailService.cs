using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Ube.Application.Features.Notifications.Email;
using Ube.Application.Common.Models;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendVerificationEmailAsync(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(_settings.Username) ||
            string.IsNullOrWhiteSpace(_settings.Password) ||
            _settings.Username.Contains("my_email@", StringComparison.OrdinalIgnoreCase) ||
            _settings.Password.Contains("my_app_password", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("SMTP credentials are not configured. Set EmailSettings Username and Password to valid values.");
        }

        var verificationLink = $"http://localhost:3000/verify-email?token={token}";

        var subject = "Verify your email";

        var body = $@"
            <h3>Welcome to Ube!</h3>
            <p>Please verify your email by clicking the link below:</p>
            <a href='{verificationLink}'>Verify Email</a>
        ";

        var message = new MailMessage
        {
            From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(email);

        using var smtp = new SmtpClient(_settings.SmtpServer, _settings.Port)
        {
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password),
            EnableSsl = _settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        await smtp.SendMailAsync(message);
    }
}