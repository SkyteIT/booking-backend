namespace Ube.Application.Features.Content.Promotion;

public interface IPromotionRepository
{
    Task<IReadOnlyList<Ube.Domain.Entities.Content.Promotion>> GetAllAsync(CancellationToken ct = default);
    Task<Ube.Domain.Entities.Content.Promotion?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Ube.Domain.Entities.Content.Promotion promotion, CancellationToken ct = default);
    Task DeleteAsync(Ube.Domain.Entities.Content.Promotion promotion, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
