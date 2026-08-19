using System.Net;
using Microsoft.Extensions.Configuration;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Notifications.Email;

namespace Ube.Application.Features.Support;

public class SupportService : ISupportService
{
    private readonly IUserRepository _userRepo;
    private readonly IEmailService _emailService;
    private readonly string _supportEmail;

    public SupportService(IUserRepository userRepo, IEmailService emailService, IConfiguration config)
    {
        _userRepo = userRepo;
        _emailService = emailService;
        _supportEmail = config["Support:Email"]
            ?? throw new InvalidOperationException("Support:Email is not configured.");
    }

    public async Task SubmitTicketAsync(Guid userId, SubmitSupportTicketDto dto, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        var body = $"""
            <p><strong>From:</strong> {WebUtility.HtmlEncode(user.FirstName)} {WebUtility.HtmlEncode(user.LastName)} ({WebUtility.HtmlEncode(user.Email)})</p>
            <p><strong>Role:</strong> {user.Role}</p>
            <p><strong>User ID:</strong> {user.Id}</p>
            <hr />
            <p>{WebUtility.HtmlEncode(dto.Message).Replace("\n", "<br />")}</p>
            """;

        await _emailService.SendEmailAsync(_supportEmail, $"[Support] {dto.Subject}", body);
    }
}
