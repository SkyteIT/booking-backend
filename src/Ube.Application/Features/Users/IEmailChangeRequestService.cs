using Ube.Application.Common.Models.Pagination;
using Ube.Domain.Enums.Users;

namespace Ube.Application.Features.Users;

public interface IEmailChangeRequestService
{
    Task<EmailChangeRequestDto> CreateAsync(Guid userId, string requestedEmail, string? reason, CancellationToken ct = default);

    Task<PagedResult<EmailChangeRequestDto>> GetPagedAsync(EmailChangeRequestStatus? status, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<EmailChangeRequestDto>> GetMineAsync(Guid userId, CancellationToken ct = default);

    Task<EmailChangeRequestDto> ApproveAsync(Guid requestId, Guid actorUserId, CancellationToken ct = default);
    Task<EmailChangeRequestDto> RejectAsync(Guid requestId, Guid actorUserId, string? notes, CancellationToken ct = default);
}
