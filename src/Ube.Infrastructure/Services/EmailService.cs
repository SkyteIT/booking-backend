using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Ube.Application.Common.Models;
using Ube.Application.Features.Notifications.Email;

namespace Ube.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Task SendVerificationEmailAsync(string email, string token)
    {
        var clientBaseUrl = string.IsNullOrWhiteSpace(_settings.ClientBaseUrl)
            ? "http://localhost:3000"
            : _settings.ClientBaseUrl.Trim().TrimEnd('/');

        var verificationLink = $"{clientBaseUrl}/verify-email?token={Uri.EscapeDataString(token)}";

        var body = $"""
            <h3>Welcome to Ube!</h3>
            <p>Please verify your email by clicking the link below:</p>
            <a href='{verificationLink}' style='display:inline-block;padding:10px 20px;
               background:#4f46e5;color:#fff;text-decoration:none;border-radius:6px;'>
               Verify Email
            </a>
            <p style='color:#6b7280;font-size:13px;margin-top:16px;'>
               This link expires in 24 hours. If you didn't create an account, you can ignore this email.
            </p>
            """;

        return SendEmailAsync(email, "Verify your Ube account", body);
    }

    public Task SendWelcomeEmailAsync(string email, string firstName)
    {
        var displayName = string.IsNullOrWhiteSpace(firstName) ? "there" : firstName.Trim();
        var body = $"""
            <h3>Welcome to Ube, {displayName}!</h3>
            <p>Your booking system account has been created successfully.</p>
            <p>You can now sign in and start using the platform.</p>
            <p style='color:#6b7280;font-size:13px;margin-top:16px;'>
               If you did not create this account, please ignore this email.
            </p>
            """;

        return SendEmailAsync(email, "Welcome to Ube", body);
    }

    public Task SendVendorApplicationSubmittedEmailAsync(string email, string firstName, string businessName)
    {
        var displayName = string.IsNullOrWhiteSpace(firstName) ? "there" : firstName.Trim();
        var safeBusinessName = string.IsNullOrWhiteSpace(businessName) ? "your business" : businessName.Trim();
        var body = $"""
            <h3>Vendor application submitted</h3>
            <p>Hi {displayName},</p>
            <p>Your vendor application for <strong>{safeBusinessName}</strong> has been submitted successfully.</p>
            <p>Status: <strong>Pending review</strong></p>
            <p>Our admin team will review your application and update you once a decision has been made.</p>
            <p style='color:#6b7280;font-size:13px;margin-top:16px;'>
               If you did not submit this application, please contact support immediately.
            </p>
            """;

        return SendEmailAsync(email, "Vendor application submitted", body);
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        ValidateSettings();

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

            using var smtp = new SmtpClient();
            smtp.ServerCertificateValidationCallback = AllowRevocationErrors;

            await smtp.ConnectAsync(_settings.SmtpServer, _settings.Port,
                _settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
            await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.Username) ||
            _settings.Username.Contains("my_email@", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("SMTP credentials are not configured.");
    }

    // Allows connections on restricted networks where revocation servers are unreachable.
    // Only blocks truly invalid certificates (wrong host, expired, self-signed without trust).
    private static bool AllowRevocationErrors(object sender, X509Certificate? certificate,
        X509Chain? chain, SslPolicyErrors errors)
    {
        if (errors == SslPolicyErrors.None) return true;
        if (chain != null)
        {
            foreach (var status in chain.ChainStatus)
            {
                if (status.Status is X509ChainStatusFlags.RevocationStatusUnknown
                                  or X509ChainStatusFlags.OfflineRevocation)
                    continue;
                return false;
            }
        }
        return true;
    }
}
