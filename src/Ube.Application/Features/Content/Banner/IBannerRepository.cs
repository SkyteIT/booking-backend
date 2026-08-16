using Ube.Domain.Entities.Content;

namespace Ube.Application.Features.Content.Banner;

public interface IBannerRepository
{
    Task<IReadOnlyList<Ube.Domain.Entities.Content.Banner>> GetAllAsync(CancellationToken ct = default);
    Task<Ube.Domain.Entities.Content.Banner?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Ube.Domain.Entities.Content.Banner banner, CancellationToken ct = default);
    Task DeleteAsync(Ube.Domain.Entities.Content.Banner banner, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
