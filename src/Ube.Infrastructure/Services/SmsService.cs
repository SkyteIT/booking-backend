using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.Extensions.Configuration;
using Ube.Application.Interfaces;

public class SmsService : ISmsService
{
    private readonly IConfiguration _config;

    public SmsService(IConfiguration config)
    {
        _config = config;

        TwilioClient.Init(
            _config["Twilio:AccountSid"],
            _config["Twilio:AuthToken"]
        );
    }

    public async Task SendSmsAsync(string to, string message)
    {
        await MessageResource.CreateAsync(
            body: message,
            from: new PhoneNumber(_config["Twilio:FromNumber"]),
            to: new PhoneNumber(to)
        );
    }
}