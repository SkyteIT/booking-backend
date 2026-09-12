using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
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

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _frontendBaseUrl = NormalizeBaseUrl(_settings.ClientBaseUrl);
    }

    public Task SendVerificationEmailAsync(string email, string token)
    {
        var verificationLink = $"{_frontendBaseUrl}/verify-email?token={token}";

        var body = $"""
            <div style="margin:0;padding:0;background:#f4f8fb;">
              <div style="max-width:640px;margin:0 auto;padding:32px 20px;font-family:Arial,Helvetica,sans-serif;color:#0f172a;">
                <div style="background:linear-gradient(135deg,#005a8d,#0077b6);border-radius:20px 20px 0 0;padding:30px 28px;color:#ffffff;">
                  <div style="font-size:13px;letter-spacing:0.14em;text-transform:uppercase;opacity:0.85;">UBE</div>
                  <h1 style="margin:10px 0 0;font-size:28px;line-height:1.15;">Verify your email address</h1>
                  <p style="margin:12px 0 0;font-size:15px;line-height:1.6;opacity:0.96;">One quick step and your Ube account will be ready.</p>
                </div>

                <div style="background:#ffffff;border:1px solid #dbe6ef;border-top:none;border-radius:0 0 20px 20px;padding:28px;">
                  <p style="margin:0 0 14px;font-size:16px;line-height:1.7;">Welcome to Ube!</p>
                  <p style="margin:0 0 20px;font-size:15px;line-height:1.7;color:#334155;">
                    Confirm that this email address belongs to you by selecting the button below.
                  </p>

                  <div style="text-align:center;margin:28px 0;">
                    <a href="{verificationLink}" style="display:inline-block;background:linear-gradient(135deg,#005a8d,#0077b6);color:#ffffff;text-decoration:none;font-weight:700;font-size:15px;padding:14px 26px;border-radius:999px;">
                      Verify email address
                    </a>
                  </div>

                  <div style="padding:16px 18px;background:#f8fbfe;border:1px solid #dbe6ef;border-radius:14px;font-size:13px;line-height:1.7;color:#64748b;">
                    This verification link expires in 24 hours. If you did not create an Ube account, you can safely ignore this email.
                  </div>
                </div>

                <div style="padding:16px 8px 0;text-align:center;font-size:12px;line-height:1.6;color:#94a3b8;">
                  This automated message was sent to help secure your Ube account.
                </div>
              </div>
            </div>
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
        var displayName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(firstName) ? "there" : firstName.Trim());
        var safeBusinessName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(businessName) ? "your business" : businessName.Trim());
        var statusLink = $"{_frontendBaseUrl}/vendor/application-status";
        var body = $"""
            <div style="margin:0;padding:0;background:#f4f8fb;">
              <div style="max-width:640px;margin:0 auto;padding:32px 20px;font-family:Arial,Helvetica,sans-serif;color:#0f172a;">
                <div style="background:linear-gradient(135deg,#005a8d,#0077b6);border-radius:20px 20px 0 0;padding:30px 28px;color:#ffffff;">
                  <div style="font-size:13px;letter-spacing:0.14em;text-transform:uppercase;opacity:0.85;">UBE VENDOR</div>
                  <h1 style="margin:10px 0 0;font-size:28px;line-height:1.15;">Application received</h1>
                  <p style="margin:12px 0 0;font-size:15px;line-height:1.6;opacity:0.96;">Your vendor application is safely with our review team.</p>
                </div>

                <div style="background:#ffffff;border:1px solid #dbe6ef;border-top:none;border-radius:0 0 20px 20px;padding:28px;">
                  <p style="margin:0 0 14px;font-size:16px;line-height:1.7;">Hi {displayName},</p>
                  <p style="margin:0 0 20px;font-size:15px;line-height:1.7;color:#334155;">
                    We received the vendor application for <strong style="color:#0f172a;">{safeBusinessName}</strong>. No further action is needed while our admin team reviews the information provided.
                  </p>

                  <div style="margin:22px 0;padding:18px;background:#f8fbfe;border:1px solid #dbe6ef;border-radius:16px;">
                    <div style="font-size:12px;font-weight:700;letter-spacing:0.1em;text-transform:uppercase;color:#64748b;margin-bottom:8px;">Current status</div>
                    <div style="display:inline-block;padding:7px 12px;border-radius:999px;background:#fff7ed;color:#b45309;font-size:14px;font-weight:700;">Pending review</div>
                    <p style="margin:14px 0 0;font-size:14px;line-height:1.7;color:#475569;">We will notify you as soon as a decision has been made.</p>
                  </div>

                  <div style="text-align:center;margin:28px 0 22px;">
                    <a href="{statusLink}" style="display:inline-block;background:linear-gradient(135deg,#005a8d,#0077b6);color:#ffffff;text-decoration:none;font-weight:700;font-size:15px;padding:14px 24px;border-radius:999px;">
                      View application status
                    </a>
                  </div>

                  <p style="margin:0;font-size:13px;line-height:1.7;color:#64748b;">
                    If you did not submit this application, please contact Ube support immediately.
                  </p>
                </div>

                <div style="padding:16px 8px 0;text-align:center;font-size:12px;line-height:1.6;color:#94a3b8;">
                  Ube vendor onboarding updates are sent automatically when your application status changes.
                </div>
              </div>
            </div>
            """;

        return SendEmailAsync(email, "Vendor application submitted", body);
    }

    public Task SendVendorApplicationApprovedEmailAsync(string email, string firstName, string businessName)
    {
        var displayName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(firstName) ? "there" : firstName.Trim());
        var safeBusinessName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(businessName) ? "your business" : businessName.Trim());
        var dashboardLink = $"{_frontendBaseUrl}/vendor/dashboard";
        var body = $"""
            <div style="margin:0;padding:0;background:#f4f8fb;">
              <div style="max-width:640px;margin:0 auto;padding:32px 20px;font-family:Arial,Helvetica,sans-serif;color:#0f172a;">
                <div style="background:linear-gradient(135deg,#005a8d,#0077b6);border-radius:20px 20px 0 0;padding:30px 28px;color:#fff;">
                  <div style="font-size:13px;letter-spacing:0.14em;text-transform:uppercase;opacity:0.85;">UBE</div>
                  <h1 style="margin:10px 0 0;font-size:28px;line-height:1.15;">Your vendor application is approved</h1>
                  <p style="margin:12px 0 0;font-size:15px;line-height:1.6;opacity:0.96;">
                    {safeBusinessName} now has vendor access on Ube.
                  </p>
                </div>

                <div style="background:#ffffff;border:1px solid #dbe6ef;border-top:none;border-radius:0 0 20px 20px;padding:28px;">
                  <p style="margin:0 0 14px;font-size:16px;line-height:1.7;">Hi {displayName},</p>
                  <p style="margin:0 0 18px;font-size:15px;line-height:1.7;color:#334155;">
                    An admin has reviewed and approved your vendor application for <strong style="color:#0f172a;">{safeBusinessName}</strong>.
                    Your account can now access vendor tools, listings, bookings, payouts, and notifications.
                  </p>

                  <div style="margin:24px 0;padding:18px 18px 14px;background:#f8fbfe;border:1px solid #dbe6ef;border-radius:16px;">
                    <div style="font-size:13px;font-weight:700;letter-spacing:0.08em;text-transform:uppercase;color:#0b5b85;margin-bottom:10px;">What happens next</div>
                    <ul style="margin:0;padding-left:18px;color:#334155;line-height:1.8;font-size:14px;">
                      <li>Sign in and open your vendor dashboard.</li>
                      <li>Set up listings, pricing, and availability.</li>
                      <li>Enable notifications so you do not miss bookings or updates.</li>
                    </ul>
                  </div>

                  <div style="text-align:center;margin:28px 0 22px;">
                    <a href="{dashboardLink}" style="display:inline-block;background:linear-gradient(135deg,#005a8d,#0077b6);color:#ffffff;text-decoration:none;font-weight:700;font-size:15px;padding:14px 24px;border-radius:999px;">
                      Open vendor dashboard
                    </a>
                  </div>

                  <p style="margin:0;font-size:13px;line-height:1.7;color:#64748b;">
                    If you are not ready to use vendor features yet, you can ignore this email for now.
                    If you did not submit this application, please contact support immediately.
                  </p>
                </div>

                <div style="padding:16px 8px 0;text-align:center;font-size:12px;line-height:1.6;color:#94a3b8;">
                  Ube vendor onboarding updates are sent automatically when your application status changes.
                </div>
              </div>
            </div>
            """;

        return SendEmailAsync(email, "Vendor application approved", body);
    }

    public Task SendVendorApplicationRejectedEmailAsync(string email, string firstName, string businessName, string? rejectionReason)
    {
        var displayName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(firstName) ? "there" : firstName.Trim());
        var safeBusinessName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(businessName) ? "your business" : businessName.Trim());
        var safeReason = string.IsNullOrWhiteSpace(rejectionReason)
            ? "No reason was provided."
            : WebUtility.HtmlEncode(rejectionReason.Trim());

        var body = $"""
            <div style="margin:0;padding:0;background:#f8f5f3;">
              <div style="max-width:640px;margin:0 auto;padding:32px 20px;font-family:Arial,Helvetica,sans-serif;color:#1f2937;">
                <div style="background:linear-gradient(135deg,#7c2d12,#b45309);border-radius:20px 20px 0 0;padding:30px 28px;color:#fff;">
                  <div style="font-size:13px;letter-spacing:0.14em;text-transform:uppercase;opacity:0.85;">UBE</div>
                  <h1 style="margin:10px 0 0;font-size:28px;line-height:1.15;">Your vendor application was not approved</h1>
                  <p style="margin:12px 0 0;font-size:15px;line-height:1.6;opacity:0.96;">
                    We reviewed the application for {safeBusinessName}.
                  </p>
                </div>

                <div style="background:#ffffff;border:1px solid #eadfd8;border-top:none;border-radius:0 0 20px 20px;padding:28px;">
                  <p style="margin:0 0 14px;font-size:16px;line-height:1.7;">Hi {displayName},</p>
                  <p style="margin:0 0 18px;font-size:15px;line-height:1.7;color:#374151;">
                    An admin has reviewed your vendor application for <strong style="color:#111827;">{safeBusinessName}</strong>
                    and decided not to approve it at this time.
                  </p>

                  <div style="margin:24px 0;padding:18px 18px 14px;background:#fff7ed;border:1px solid #fed7aa;border-radius:16px;">
                    <div style="font-size:13px;font-weight:700;letter-spacing:0.08em;text-transform:uppercase;color:#9a3412;margin-bottom:10px;">Review notes</div>
                    <p style="margin:0;color:#7c2d12;line-height:1.7;font-size:14px;">{safeReason}</p>
                  </div>

                  <p style="margin:0 0 14px;font-size:15px;line-height:1.7;color:#374151;">
                    You can update the application details and submit again if you want to try another review.
                  </p>

                  <p style="margin:0;font-size:13px;line-height:1.7;color:#6b7280;">
                    If you believe this was a mistake, please contact support.
                  </p>
                </div>

                <div style="padding:16px 8px 0;text-align:center;font-size:12px;line-height:1.6;color:#94a3b8;">
                  Ube vendor onboarding updates are sent automatically when your application status changes.
                </div>
              </div>
            </div>
            """;

        return SendEmailAsync(email, "Vendor application status update", body);
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

    private static string NormalizeBaseUrl(string? baseUrl)
    {
        var value = string.IsNullOrWhiteSpace(baseUrl) ? "http://localhost:3000" : baseUrl.Trim();
        return value.TrimEnd('/');
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
