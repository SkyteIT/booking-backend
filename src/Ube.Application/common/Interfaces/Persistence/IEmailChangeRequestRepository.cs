using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Users;

namespace Ube.Application.Common.Interfaces.Persistence;

public interface IEmailChangeRequestRepository
{
    Task<EmailChangeRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<EmailChangeRequest>> GetByRequesterAsync(Guid userId, CancellationToken ct = default);
    Task<bool> HasPendingForUserAsync(Guid userId, CancellationToken ct = default);
    Task<(List<EmailChangeRequest> Items, int TotalCount)> GetPagedAsync(
        EmailChangeRequestStatus? status, int pageNumber, int pageSize, CancellationToken ct = default);
    Task AddAsync(EmailChangeRequest request, CancellationToken ct = default);
    Task UpdateAsync(EmailChangeRequest request, CancellationToken ct = default);
}
