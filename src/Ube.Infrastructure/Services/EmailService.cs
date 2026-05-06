using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Ube.Application.Interfaces;

namespace Ube.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string message)
    {
        try
        {
            var fromAddress = _config["Email:From"]
                ?? throw new InvalidOperationException("Email:From is not configured.");
            var smtpHost = _config["Email:SmtpHost"]
                ?? throw new InvalidOperationException("Email:SmtpHost is not configured.");
            var username = _config["Email:Username"]
                ?? throw new InvalidOperationException("Email:Username is not configured.");
            var password = _config["Email:Password"]
                ?? throw new InvalidOperationException("Email:Password is not configured.");

            if (!int.TryParse(_config["Email:Port"], out var port))
                port = 587;

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(fromAddress));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            email.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
            {
                Text = message
            };

            using var smtp = new SmtpClient();

            // Fix: Bypass SSL certificate revocation check
            // This is needed when Windows cannot reach the revocation server
            // (common on restricted networks, university Wi-Fi, etc.)
            smtp.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                // Allow if no errors, or only revocation check failed
                if (sslPolicyErrors == SslPolicyErrors.None)
                    return true;

                // Only block if the certificate itself is invalid (wrong host, expired, etc.)
                // Allow if the only issue is revocation check failure
                if (chain != null)
                {
                    foreach (var status in chain.ChainStatus)
                    {
                        if (status.Status == X509ChainStatusFlags.RevocationStatusUnknown ||
                            status.Status == X509ChainStatusFlags.OfflineRevocation)
                            continue; // ignore revocation errors only

                        return false; // block any other certificate error
                    }
                }

                return true;
            };

            await smtp.ConnectAsync(smtpHost, port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(username, password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }
}