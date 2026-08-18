using Ube.Application.Common.Models.Pagination;
using Ube.Domain.Enums.Users;

namespace Ube.Application.Features.Users;

public interface IRoleChangeRequestService
{
    // Called by AdminService when the actor is a plain Admin, not a
    // SuperAdmin - never mutates the target's role, only records the ask.
    Task<RoleChangeRequestDto> CreateAsync(Guid targetUserId, UserRole requestedRole, Guid requestedByUserId, string? reason, CancellationToken ct = default);

    Task<PagedResult<RoleChangeRequestDto>> GetPagedAsync(RoleChangeRequestStatus? status, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<RoleChangeRequestDto>> GetMineAsync(Guid requestedByUserId, CancellationToken ct = default);

    Task<RoleChangeRequestDto> ApproveAsync(Guid requestId, Guid actorUserId, CancellationToken ct = default);
    Task<RoleChangeRequestDto> RejectAsync(Guid requestId, Guid actorUserId, string? notes, CancellationToken ct = default);
}
