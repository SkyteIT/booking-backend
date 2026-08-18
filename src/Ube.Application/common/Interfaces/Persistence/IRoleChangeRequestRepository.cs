using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Users;

namespace Ube.Application.Common.Interfaces.Persistence;

public interface IRoleChangeRequestRepository
{
    Task<RoleChangeRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<RoleChangeRequest>> GetByRequesterAsync(Guid requestedByUserId, CancellationToken ct = default);
    Task<(List<RoleChangeRequest> Items, int TotalCount)> GetPagedAsync(
        RoleChangeRequestStatus? status, int pageNumber, int pageSize, CancellationToken ct = default);
    Task AddAsync(RoleChangeRequest request, CancellationToken ct = default);
    Task UpdateAsync(RoleChangeRequest request, CancellationToken ct = default);
}
