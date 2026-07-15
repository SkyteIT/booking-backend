using Ube.Domain.Entities.Content;
using Ube.Domain.Enums;
using Ube.Domain.Enums.Content;

namespace Ube.Application.Features.Content.Promotion;

public class PromotionService : IPromotionService
{
    private readonly IPromotionRepository _repo;

    public PromotionService(IPromotionRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<PromotionDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var promotions = await _repo.GetAllAsync(cancellationToken);
        return promotions.Select(ToDto).ToList();
    }

    public async Task<PromotionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var promotion = await _repo.GetByIdAsync(id, cancellationToken);
        return promotion is null ? null : ToDto(promotion);
    }

    public async Task<PromotionDto> CreateAsync(CreatePromotionDto dto, CancellationToken cancellationToken)
    {
        var promotion = new Ube.Domain.Entities.Content.Promotion
        {
            PromoCode = dto.PromoCode,
            Type = (PromotionType)dto.Type,
            Value = dto.Value,
            UsageLimit = dto.UsageLimit,
            UsageCount = 0,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = RecordStatus.Active
        };

        await _repo.AddAsync(promotion, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        return ToDto(promotion);
    }

    public async Task<PromotionDto?> UpdateAsync(Guid id, UpdatePromotionDto dto, CancellationToken cancellationToken)
    {
        var promotion = await _repo.GetByIdAsync(id, cancellationToken);
        if (promotion is null) return null;

        promotion.PromoCode = dto.PromoCode;
        promotion.Type = (PromotionType)dto.Type;
        promotion.Value = dto.Value;
        promotion.UsageCount = dto.UsageCount;
        promotion.UsageLimit = dto.UsageLimit;
        promotion.StartDate = dto.StartDate;
        promotion.EndDate = dto.EndDate;
        promotion.Status = (RecordStatus)dto.Status;
        promotion.UpdatedAt = DateTime.UtcNow;

        await _repo.SaveChangesAsync(cancellationToken);

        return ToDto(promotion);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var promotion = await _repo.GetByIdAsync(id, cancellationToken);
        if (promotion is null) return false;

        await _repo.DeleteAsync(promotion, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static PromotionDto ToDto(Ube.Domain.Entities.Content.Promotion x) => new()
    {
        Id = x.Id,
        PromoCode = x.PromoCode,
        Type = x.Type.ToString(),
        Value = x.Value,
        UsageCount = x.UsageCount,
        UsageLimit = x.UsageLimit,
        StartDate = x.StartDate,
        EndDate = x.EndDate,
        Status = x.Status.ToString()
    };
}
