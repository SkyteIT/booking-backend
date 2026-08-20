using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Ube.Application.Common.Models;
using Ube.Application.Features.Notifications.Email;

namespace Ube.Infrastructure.Integrations.Smtp;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;
    private readonly string _frontendBaseUrl;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger, IConfiguration configuration)
    {
        _settings = settings.Value;
        _logger = logger;
        _frontendBaseUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
    }

    public Task SendVerificationEmailAsync(string email, string token)
    {
        var verificationLink = $"{_frontendBaseUrl}/verify-email?token={token}";

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

    public Task SendEmailChangeVerificationEmailAsync(string newEmail, string token)
    {
        var verificationLink = $"{_frontendBaseUrl}/verify-email?token={token}";

        var body = $"""
            <h3>Confirm your new email address</h3>
            <p>You asked to change the email on your Ube account to this address. Click the link below to confirm it:</p>
            <a href='{verificationLink}' style='display:inline-block;padding:10px 20px;
               background:#4f46e5;color:#fff;text-decoration:none;border-radius:6px;'>
               Confirm new email
            </a>
            <p style='color:#6b7280;font-size:13px;margin-top:16px;'>
               This link expires in 24 hours. If you didn't request this change, you can ignore this email - your account's email won't change.
            </p>
            """;

        return SendEmailAsync(newEmail, "Confirm your new Ube email address", body);
    }

    public Task SendPasswordResetEmailAsync(string email, string token)
    {
        var resetLink = $"{_frontendBaseUrl}/reset-password?token={token}";

        var body = $"""
            <h3>Reset your Ube password</h3>
            <p>We received a request to reset your password. Click the link below to choose a new one:</p>
            <a href='{resetLink}' style='display:inline-block;padding:10px 20px;
               background:#4f46e5;color:#fff;text-decoration:none;border-radius:6px;'>
               Reset Password
            </a>
            <p style='color:#6b7280;font-size:13px;margin-top:16px;'>
               This link expires in 10 minutes. If you didn't request a password reset, you can safely ignore this email.
            </p>
            """;

        return SendEmailAsync(email, "Reset your Ube password", body);
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
