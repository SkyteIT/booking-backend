using Ube.Domain.Entities.Content;
using Ube.Domain.Enums.Content;

namespace Ube.Application.Features.Content.Banner;

public interface IBannerRepository
{
    Task<IReadOnlyList<Ube.Domain.Entities.Content.Banner>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Ube.Domain.Entities.Content.Banner>> GetActiveByPlacementAsync(
        BannerPlacement placement,
        DateOnly asOfDate,
        CancellationToken ct = default);
    Task<Ube.Domain.Entities.Content.Banner?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Ube.Domain.Entities.Content.Banner banner, CancellationToken ct = default);
    Task DeleteAsync(Ube.Domain.Entities.Content.Banner banner, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
