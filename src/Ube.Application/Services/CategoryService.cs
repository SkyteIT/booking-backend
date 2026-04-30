using Microsoft.EntityFrameworkCore;
using Ube.Application.DTOs.Category;
using Ube.Application.Interfaces;
using Ube.Domain.Entities.Content;
using Ube.Domain.Enums;

namespace Ube.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IAppDbContext _context;

    public CategoryService(IAppDbContext context)
    {
        _context = context;
    }

    // ── Shared projection ──────────────────────────────────────────────────
    private static CategoryDto ToDto(Category x, int listingCount) => new()
    {
        Id = x.Id,
        Name = x.Name,
        Description = x.Description,
        BookingType = x.BookingType,
        ServiceModel = x.ServiceModel,
        DateSelectionEnabled = x.DateSelectionEnabled,
        TimeSlotEnabled = x.TimeSlotEnabled,
        AvailabilityCalendarEnabled = x.AvailabilityCalendarEnabled,
        DefaultCommissionPercent = x.DefaultCommissionPercent,
        PlatformServiceFee = x.PlatformServiceFee,
        TaxApplicable = x.TaxApplicable,
        Icon = x.Icon,
        BannerImageUrl = x.BannerImageUrl,
        DisplayOrder = x.DisplayOrder,
        IsFeatured = x.IsFeatured,
        RequiresAdminApproval = x.RequiresAdminApproval,
        Status = x.Status.ToString(),
        ListingCount = listingCount,
        CreatedAtUtc = x.CreatedAtUtc,
        UpdatedAtUtc = x.UpdatedAtUtc,
    };

    private static RecordStatus ParseStatus(string? raw, RecordStatus fallback = RecordStatus.Active)
    {
        if (Enum.TryParse<RecordStatus>(raw, ignoreCase: true, out var parsed))
            return parsed;
        return fallback;
    }

    // ── GetAll ─────────────────────────────────────────────────────────────
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var categories = await _context.Categories
            .Where(x => x.Status != RecordStatus.Deleted)
            .Include(x => x.Listings)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);

        return categories.Select(x => ToDto(x, x.Listings.Count)).ToList();
    }

    // ── GetFiltered (search + status filter for the redesigned UI) ─────────
    public async Task<IReadOnlyList<CategoryDto>> GetFilteredAsync(
        string? status, string? search, CancellationToken cancellationToken)
    {
        var query = _context.Categories
            .Where(x => x.Status != RecordStatus.Deleted)
            .Include(x => x.Listings)
            .AsQueryable();

        // Status filter
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<RecordStatus>(status, ignoreCase: true, out var recordStatus))
        {
            query = query.Where(x => x.Status == recordStatus);
        }

        // Search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(lower));
        }

        var categories = await query.OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);
        return categories.Select(x => ToDto(x, x.Listings.Count)).ToList();
    }

    // ── GetById ────────────────────────────────────────────────────────────
    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var x = await _context.Categories
            .Include(x => x.Listings)
            .FirstOrDefaultAsync(x => x.Id == id && x.Status != RecordStatus.Deleted, cancellationToken);

        return x is null ? null : ToDto(x, x.Listings.Count);
    }

    // ── Create ─────────────────────────────────────────────────────────────
    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken)
    {
        // Guard: reject duplicate names up-front with a clear 409 message
        var nameExists = await _context.Categories
            .AnyAsync(x => x.Name.ToLower() == dto.Name.ToLower().Trim() &&
                           x.Status != RecordStatus.Deleted, cancellationToken);

        if (nameExists)
            throw new InvalidOperationException($"A category named '{dto.Name}' already exists.");

        var entity = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            BookingType = dto.BookingType,
            ServiceModel = dto.ServiceModel,
            DateSelectionEnabled = dto.DateSelectionEnabled,
            TimeSlotEnabled = dto.TimeSlotEnabled,
            AvailabilityCalendarEnabled = dto.AvailabilityCalendarEnabled,
            DefaultCommissionPercent = dto.DefaultCommissionPercent,
            PlatformServiceFee = dto.PlatformServiceFee,
            TaxApplicable = dto.TaxApplicable,
            Icon = dto.Icon,
            BannerImageUrl = dto.BannerImageUrl,
            DisplayOrder = dto.DisplayOrder,
            IsFeatured = dto.IsFeatured,
            RequiresAdminApproval = dto.RequiresAdminApproval,
            Status = ParseStatus(dto.Status),
        };

        _context.Categories.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(entity, 0);
    }

    // ── Update ─────────────────────────────────────────────────────────────
    public async Task<CategoryDto?> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken)
    {
        var entity = await _context.Categories
            .Include(x => x.Listings)
            .FirstOrDefaultAsync(x => x.Id == id && x.Status != RecordStatus.Deleted, cancellationToken);

        if (entity is null) return null;

        if (dto.Name is not null)                       entity.Name = dto.Name;
        if (dto.Description is not null)                entity.Description = dto.Description;
        if (dto.BookingType is not null)                entity.BookingType = dto.BookingType;
        if (dto.ServiceModel is not null)               entity.ServiceModel = dto.ServiceModel;
        if (dto.DateSelectionEnabled.HasValue)          entity.DateSelectionEnabled = dto.DateSelectionEnabled.Value;
        if (dto.TimeSlotEnabled.HasValue)               entity.TimeSlotEnabled = dto.TimeSlotEnabled.Value;
        if (dto.AvailabilityCalendarEnabled.HasValue)   entity.AvailabilityCalendarEnabled = dto.AvailabilityCalendarEnabled.Value;
        if (dto.DefaultCommissionPercent.HasValue)      entity.DefaultCommissionPercent = dto.DefaultCommissionPercent.Value;
        if (dto.PlatformServiceFee.HasValue)            entity.PlatformServiceFee = dto.PlatformServiceFee.Value;
        if (dto.TaxApplicable.HasValue)                 entity.TaxApplicable = dto.TaxApplicable.Value;
        if (dto.Icon is not null)                       entity.Icon = dto.Icon;
        if (dto.BannerImageUrl is not null)             entity.BannerImageUrl = dto.BannerImageUrl;
        if (dto.DisplayOrder.HasValue)                  entity.DisplayOrder = dto.DisplayOrder.Value;
        if (dto.IsFeatured.HasValue)                    entity.IsFeatured = dto.IsFeatured.Value;
        if (dto.RequiresAdminApproval.HasValue)         entity.RequiresAdminApproval = dto.RequiresAdminApproval.Value;
        if (dto.Status is not null)                     entity.Status = ParseStatus(dto.Status, entity.Status);

        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(entity, entity.Listings.Count);
    }

    // ── Delete (hard) ──────────────────────────────────────────────────────
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _context.Categories
            .Include(x => x.Listings)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null) return false;

        // Remove all child listings first (FK is Restrict — must delete children before parent)
        if (entity.Listings.Any())
            _context.Listings.RemoveRange(entity.Listings);

        // Hard delete — physically removes the row from the database
        _context.Categories.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ── Toggle status (Active ↔ Inactive) ──────────────────────────────────
    public async Task<CategoryDto?> ToggleStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var entity = await _context.Categories
            .Include(x => x.Listings)
            .FirstOrDefaultAsync(x => x.Id == id && x.Status != RecordStatus.Deleted, cancellationToken);

        if (entity is null) return null;

        entity.Status = isActive ? RecordStatus.Active : RecordStatus.Inactive;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(entity, entity.Listings.Count);
    }
}
