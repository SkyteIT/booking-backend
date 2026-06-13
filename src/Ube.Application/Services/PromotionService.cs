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

    // ── Shared mapper: entity → DTO ─────────────────────────────────────────
    // Centralised so all methods return a consistent shape.
    // IsActive is derived from the Status enum so the frontend's
    // derivePromotionStatus() helper receives a reliable bool.
    private static PromotionDto MapToDto(Promotion x) => new()
    {
        Id = x.Id,
        Code = x.PromoCode,                          // entity field → DTO field "Code"
        PromotionType = (int)x.Type,                          // enum → int (0=Percentage, 1=Fixed)
        DiscountValue = x.Value,
        UsageCount = x.UsageCount,
        UsageLimit = x.UsageLimit,
        StartDate = x.StartDate,
        EndDate = x.EndDate,
        IsActive = x.Status == RecordStatus.Active,      // bool for frontend
    };

    // ── Get all promotions (latest first) ───────────────────────────────────
    public async Task<IReadOnlyList<PromotionDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var promotions = await _context.Promotions
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return promotions.Select(MapToDto).ToList();
    }

    // ── Get promotion by ID ─────────────────────────────────────────────────
    public async Task<PromotionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var promotion = await _context.Promotions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return promotion is null ? null : MapToDto(promotion);
    }

    // ── Create new promotion ────────────────────────────────────────────────
    public async Task<PromotionDto> CreateAsync(CreatePromotionDto dto, CancellationToken cancellationToken)
    {
        // FIX: normalise code before the duplicate check so "promo1" and "PROMO1"
        //      are treated as the same code, matching the unique index behaviour.
        var normalizedCode = dto.Code.Trim().ToUpperInvariant();

        // FIX: check for duplicate BEFORE SaveChanges to return a clean 409
        //      instead of crashing with a SqlException from the unique index.
        var exists = await _context.Promotions
            .AnyAsync(p => p.PromoCode == normalizedCode, cancellationToken);

        if (exists)
            // GlobalExceptionMiddleware maps InvalidOperationException → 409 Conflict
            throw new InvalidOperationException($"Promo code \"{normalizedCode}\" already exists.");

        var promotion = new Promotion
        {
            PromoCode = normalizedCode,
            Type = (PromotionType)dto.PromotionType,   // FIX: use PromotionType not Type
            Value = dto.DiscountValue,                   // FIX: use DiscountValue not Value
            UsageLimit = dto.UsageLimit,
            UsageCount = 0,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = RecordStatus.Active,                 // new promos are always Active
        };

        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(promotion);
    }

    // ── Update existing promotion ───────────────────────────────────────────
    public async Task<PromotionDto?> UpdateAsync(Guid id, UpdatePromotionDto dto, CancellationToken cancellationToken)
    {
        var promotion = await _context.Promotions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (promotion is null) return null;

        var normalizedCode = dto.Code.Trim().ToUpperInvariant();

        // FIX: check for duplicate code only if it changed (avoid false conflict
        //      when updating other fields on the same record).
        if (!string.Equals(promotion.PromoCode, normalizedCode, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _context.Promotions
                .AnyAsync(p => p.PromoCode == normalizedCode, cancellationToken);

            if (exists)
                throw new InvalidOperationException($"Promo code \"{normalizedCode}\" already exists.");
        }

        promotion.PromoCode = normalizedCode;
        promotion.Type = (PromotionType)dto.PromotionType;  // FIX: use PromotionType
        promotion.Value = dto.DiscountValue;                  // FIX: use DiscountValue
        promotion.UsageCount = dto.UsageCount;
        promotion.UsageLimit = dto.UsageLimit;
        promotion.StartDate = dto.StartDate;
        promotion.EndDate = dto.EndDate;
        promotion.Status = dto.IsActive                        // FIX: use IsActive bool
            ? RecordStatus.Active
            : RecordStatus.Inactive;
        promotion.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(promotion);
    }

    // ── Delete promotion ────────────────────────────────────────────────────
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var promotion = await _context.Promotions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (promotion is null) return false;

        _context.Promotions.Remove(promotion);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}