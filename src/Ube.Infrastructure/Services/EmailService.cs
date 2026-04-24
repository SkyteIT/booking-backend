using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Ube.Application.Interfaces;

namespace Ube.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string to, string subject, string message)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_config["Email:From"]!));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;

        email.Body = new TextPart("plain") { Text = message };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(
             _config["Email:SmtpHost"]!,
             int.Parse(_config["Email:Port"]!),
             false
        );

        await smtp.AuthenticateAsync(
            _config["Email:Username"]!,
            _config["Email:Password"]!
        );

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
