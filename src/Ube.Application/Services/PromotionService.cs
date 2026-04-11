using Microsoft.EntityFrameworkCore;
using Ube.Application.DTOs.Promotion;
using Ube.Application.Interfaces;
using Ube.Domain.Entities.Content;
using Ube.Domain.Enums;

namespace Ube.Application.Services;

public class PromotionService : IPromotionService
{
    private readonly IAppDbContext _context;

    public PromotionService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PromotionDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Promotions
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new PromotionDto
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
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PromotionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Promotions
            .Where(x => x.Id == id)
            .Select(x => new PromotionDto
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
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PromotionDto> CreateAsync(CreatePromotionDto dto, CancellationToken cancellationToken)
    {
        var promotion = new Promotion
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

        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync(cancellationToken);

        return new PromotionDto
        {
            Id = promotion.Id,
            PromoCode = promotion.PromoCode,
            Type = promotion.Type.ToString(),
            Value = promotion.Value,
            UsageCount = promotion.UsageCount,
            UsageLimit = promotion.UsageLimit,
            StartDate = promotion.StartDate,
            EndDate = promotion.EndDate,
            Status = promotion.Status.ToString()
        };
    }

    public async Task<PromotionDto?> UpdateAsync(Guid id, UpdatePromotionDto dto, CancellationToken cancellationToken)
    {
        var promotion = await _context.Promotions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (promotion is null) return null;

        promotion.PromoCode = dto.PromoCode;
        promotion.Type = (PromotionType)dto.Type;
        promotion.Value = dto.Value;
        promotion.UsageCount = dto.UsageCount;
        promotion.UsageLimit = dto.UsageLimit;
        promotion.StartDate = dto.StartDate;
        promotion.EndDate = dto.EndDate;
        promotion.Status = (RecordStatus)dto.Status;
        promotion.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new PromotionDto
        {
            Id = promotion.Id,
            PromoCode = promotion.PromoCode,
            Type = promotion.Type.ToString(),
            Value = promotion.Value,
            UsageCount = promotion.UsageCount,
            UsageLimit = promotion.UsageLimit,
            StartDate = promotion.StartDate,
            EndDate = promotion.EndDate,
            Status = promotion.Status.ToString()
        };
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var promotion = await _context.Promotions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (promotion is null) return false;

        _context.Promotions.Remove(promotion);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
