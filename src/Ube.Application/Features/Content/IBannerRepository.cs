using Ube.Domain.Entities.Content;

namespace Ube.Application.Features.Content;

public interface IBannerRepository
{
    Task<IReadOnlyList<Banner>> GetAllAsync(CancellationToken ct = default);
    Task<Banner?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Banner banner, CancellationToken ct = default);
    Task DeleteAsync(Banner banner, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
