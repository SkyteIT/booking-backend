
namespace Ube.Application.Features.Content.Promotion;

public interface IPromotionService
{
    Task<IReadOnlyList<PromotionDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<PromotionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PromotionDto> CreateAsync(CreatePromotionDto dto, CancellationToken cancellationToken);
    Task<PromotionDto?> UpdateAsync(Guid id, UpdatePromotionDto dto, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
