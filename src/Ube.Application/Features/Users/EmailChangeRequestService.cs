using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Models.Pagination;
using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Users;

namespace Ube.Application.Features.Users;

public class EmailChangeRequestService : IEmailChangeRequestService
{
    private readonly IEmailChangeRequestRepository _requestRepo;
    private readonly IUserRepository _userRepo;

    public EmailChangeRequestService(IEmailChangeRequestRepository requestRepo, IUserRepository userRepo)
    {
        _requestRepo = requestRepo;
        _userRepo = userRepo;
    }

    public async Task<EmailChangeRequestDto> CreateAsync(Guid userId, string requestedEmail, string? reason, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        var normalized = requestedEmail.Trim().ToLower();

        if (normalized == user.Email.Trim().ToLower())
            throw new BusinessRuleException("That is already your current email address.");

        if (await _userRepo.ExistsByEmailAsync(normalized))
            throw new BusinessRuleException("That email address is already in use.");

        if (await _requestRepo.HasPendingForUserAsync(userId, ct))
            throw new BusinessRuleException("You already have a pending email change request.");

        var request = new EmailChangeRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CurrentEmail = user.Email,
            RequestedEmail = normalized,
            Reason = reason,
            Status = EmailChangeRequestStatus.Pending
        };

        await _requestRepo.AddAsync(request, ct);
        request.User = user;
        return ToDto(request);
    }

    public async Task<PagedResult<EmailChangeRequestDto>> GetPagedAsync(EmailChangeRequestStatus? status, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var (items, totalCount) = await _requestRepo.GetPagedAsync(status, pageNumber, pageSize, ct);

        return new PagedResult<EmailChangeRequestDto>
        {
            Items = items.Select(ToDto).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<IReadOnlyList<EmailChangeRequestDto>> GetMineAsync(Guid userId, CancellationToken ct = default)
    {
        var requests = await _requestRepo.GetByRequesterAsync(userId, ct);
        return requests.Select(ToDto).ToList();
    }

    public async Task<EmailChangeRequestDto> ApproveAsync(Guid requestId, Guid actorUserId, CancellationToken ct = default)
    {
        var request = await _requestRepo.GetByIdAsync(requestId, ct)
            ?? throw new NotFoundException("Email change request not found");

        if (request.Status != EmailChangeRequestStatus.Pending)
            throw new BusinessRuleException("Only a pending request can be approved.");

        // Re-check uniqueness at approval time - another account may have
        // taken the email since the request was submitted.
        if (await _userRepo.ExistsByEmailAsync(request.RequestedEmail))
            throw new BusinessRuleException("That email address is already in use.");

        request.User.Email = request.RequestedEmail;
        request.User.UpdatedAt = DateTime.UtcNow;
        await _userRepo.UpdateAsync(request.User);

        request.Status = EmailChangeRequestStatus.Approved;
        request.ReviewedByUserId = actorUserId;
        request.ReviewedAt = DateTime.UtcNow;
        await _requestRepo.UpdateAsync(request, ct);

        return ToDto(request);
    }

    public async Task<EmailChangeRequestDto> RejectAsync(Guid requestId, Guid actorUserId, string? notes, CancellationToken ct = default)
    {
        var request = await _requestRepo.GetByIdAsync(requestId, ct)
            ?? throw new NotFoundException("Email change request not found");

        if (request.Status != EmailChangeRequestStatus.Pending)
            throw new BusinessRuleException("Only a pending request can be rejected.");

        request.Status = EmailChangeRequestStatus.Rejected;
        request.ReviewedByUserId = actorUserId;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewNotes = notes;
        await _requestRepo.UpdateAsync(request, ct);

        return ToDto(request);
    }

    private static EmailChangeRequestDto ToDto(EmailChangeRequest r) => new()
    {
        Id = r.Id,
        UserId = r.UserId,
        UserName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : string.Empty,
        CurrentEmail = r.CurrentEmail,
        RequestedEmail = r.RequestedEmail,
        Reason = r.Reason,
        Status = r.Status,
        ReviewedByUserId = r.ReviewedByUserId,
        ReviewedAt = r.ReviewedAt,
        ReviewNotes = r.ReviewNotes,
        CreatedAt = r.CreatedAt
    };
}
