using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Notifications;
using Ube.Domain.Entities.Content;
using Ube.Domain.Enums;
using Ube.Domain.Enums.Notifications;
using Ube.Domain.Enums.Content;
using Ube.Domain.Enums.Users;

namespace Ube.Application.Features.Content.Promotion;

public class PromotionService : IPromotionService
{
    private readonly IPromotionRepository _repo;
    private readonly IUserRepository _userRepo;
    private readonly INotificationService _notificationService;

    public PromotionService(
        IPromotionRepository repo,
        IUserRepository userRepo,
        INotificationService notificationService)
    {
        _repo = repo;
        _userRepo = userRepo;
        _notificationService = notificationService;
    }

    // ── Mapper: Entity → DTO ────────────────────────────────────────────────
    private static PromotionDto MapToDto(Ube.Domain.Entities.Content.Promotion x) => new()
    {
        Id = x.Id,
        Code = x.PromoCode,
        PromotionType = (int)x.Type,
        DiscountValue = x.Value,
        UsageCount = x.UsageCount,
        UsageLimit = x.UsageLimit,
        StartDate = x.StartDate,
        EndDate = x.EndDate,
        IsActive = x.Status == RecordStatus.Active
    };

    // ── Get all promotions ──────────────────────────────────────────────────
    public async Task<IReadOnlyList<PromotionDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var promotions = await _repo.GetAllAsync(cancellationToken);
        return promotions
            .OrderByDescending(x => x.CreatedAt)
            .Select(MapToDto)
            .ToList();
    }

    // ── Get by id ───────────────────────────────────────────────────────────
    public async Task<PromotionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var promotion = await _repo.GetByIdAsync(id, cancellationToken);
        return promotion is null ? null : MapToDto(promotion);
    }

    // ── Create promotion ────────────────────────────────────────────────────
    public async Task<PromotionDto> CreateAsync(CreatePromotionDto dto, CancellationToken cancellationToken)
    {
        var normalizedCode = dto.Code.Trim().ToUpperInvariant();

        var all = await _repo.GetAllAsync(cancellationToken);
        var exists = all.Any(p => p.PromoCode == normalizedCode);

        if (exists)
            throw new InvalidOperationException($"Promo code \"{normalizedCode}\" already exists.");

        var promotion = new Ube.Domain.Entities.Content.Promotion
        {
            PromoCode = normalizedCode,
            Type = (PromotionType)dto.PromotionType,
            Value = dto.DiscountValue,
            UsageLimit = dto.UsageLimit,
            UsageCount = 0,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = RecordStatus.Active
        };

        await _repo.AddAsync(promotion, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        await NotifyCustomersAsync(
            NotificationType.CustomerPromotionAndOfferAvailable,
            "Promotion available",
            $"A new promotion is available: {promotion.PromoCode}.",
            cancellationToken);

        return MapToDto(promotion);
    }

    // ── Update promotion ────────────────────────────────────────────────────
    public async Task<PromotionDto?> UpdateAsync(Guid id, UpdatePromotionDto dto, CancellationToken cancellationToken)
    {
        var promotion = await _repo.GetByIdAsync(id, cancellationToken);
        if (promotion is null) return null;

        var normalizedCode = dto.Code.Trim().ToUpperInvariant();

        var all = await _repo.GetAllAsync(cancellationToken);

        if (!string.Equals(promotion.PromoCode, normalizedCode, StringComparison.OrdinalIgnoreCase))
        {
            var exists = all.Any(p => p.PromoCode == normalizedCode);

            if (exists)
                throw new InvalidOperationException($"Promo code \"{normalizedCode}\" already exists.");
        }

        promotion.PromoCode = normalizedCode;
        promotion.Type = (PromotionType)dto.PromotionType;
        promotion.Value = dto.DiscountValue;
        promotion.UsageCount = dto.UsageCount;
        promotion.UsageLimit = dto.UsageLimit;
        promotion.StartDate = dto.StartDate;
        promotion.EndDate = dto.EndDate;

        promotion.Status = dto.IsActive
            ? RecordStatus.Active
            : RecordStatus.Inactive;

        promotion.UpdatedAt = DateTime.UtcNow;

        await _repo.SaveChangesAsync(cancellationToken);

        if (promotion.Status == RecordStatus.Active)
        {
            await NotifyCustomersAsync(
                NotificationType.CustomerPromotionAndOfferAvailable,
                "Promotion updated",
                $"A promotion has been updated: {promotion.PromoCode}.",
                cancellationToken);
        }

        return MapToDto(promotion);
    }

    // ── Delete promotion ────────────────────────────────────────────────────
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var promotion = await _repo.GetByIdAsync(id, cancellationToken);
        if (promotion is null) return false;

        await _repo.DeleteAsync(promotion, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task NotifyCustomersAsync(
        NotificationType type,
        string title,
        string message,
        CancellationToken cancellationToken)
    {
        var customers = await _userRepo.GetByRoleAsync(UserRole.User);

        foreach (var customer in customers)
        {
            try
            {
                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    UserId = customer.Id,
                    Title = title,
                    Message = message,
                    Type = (int)type
                }, cancellationToken);
            }
            catch
            {
                // best-effort only
            }
        }
    }
}
