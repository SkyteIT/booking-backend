using Ube.Domain.Entities.Content;

namespace Ube.Application.Features.Content;

public interface IPromotionRepository
{
    Task<IReadOnlyList<Promotion>> GetAllAsync(CancellationToken ct = default);
    Task<Promotion?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Promotion promotion, CancellationToken ct = default);
    Task DeleteAsync(Promotion promotion, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
