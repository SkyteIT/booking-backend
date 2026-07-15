using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ube.Application.Features.Notifications;

namespace Ube.Infrastructure.Integrations.Sms;

public class SmsService : ISmsService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmsService> _logger;

    public SmsService(IConfiguration config, ILogger<SmsService> logger)
    {
        _config = config;
        _logger = logger;

        var accountSid = _config["Twilio:AccountSid"]
            ?? throw new InvalidOperationException("Twilio:AccountSid is not configured.");
        var authToken = _config["Twilio:AuthToken"]
            ?? throw new InvalidOperationException("Twilio:AuthToken is not configured.");

        TwilioClient.Init(accountSid, authToken);
    }

    public async Task SendSmsAsync(string to, string message)
    {
        try
        {
            var fromNumber = _config["Twilio:FromNumber"]
                ?? throw new InvalidOperationException("Twilio:FromNumber is not configured.");

            var result = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(fromNumber),
                to: new PhoneNumber(to)
            );

            _logger.LogInformation(
                "SMS sent to {To}, Twilio SID: {Sid}, Status: {Status}",
                to, result.Sid, result.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {To}", to);
            throw;
        }
    }
}
