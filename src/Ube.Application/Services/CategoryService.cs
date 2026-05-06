using Microsoft.EntityFrameworkCore;
using Ube.Application.DTOs.Category;
using Ube.Application.Interfaces;
using Ube.Domain.Constants;
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
            .Where(x => x.Status != RecordStatus.Deleted
                     && x.Name != CategoryConstants.UncategorizedName)
            .Include(x => x.Listings)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);

        return categories.Select(x => ToDto(x, x.Listings.Count)).ToList();
    }

    // ── GetFiltered ────────────────────────────────────────────────────────
    public async Task<IReadOnlyList<CategoryDto>> GetFilteredAsync(
        string? status, string? search, CancellationToken cancellationToken)
    {
        var query = _context.Categories
            .Where(x => x.Status != RecordStatus.Deleted
                     && x.Name != CategoryConstants.UncategorizedName)
            .Include(x => x.Listings)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<RecordStatus>(status, ignoreCase: true, out var recordStatus))
        {
            query = query.Where(x => x.Status == recordStatus);
        }

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
        var trimmedName = dto.Name.Trim();

        // Block if an active (non-deleted) category with this name already exists
        var activeExists = await _context.Categories
            .AnyAsync(x => x.Name.ToLower() == trimmedName.ToLower() &&
                           x.Status != RecordStatus.Deleted, cancellationToken);

        if (activeExists)
            throw new InvalidOperationException($"A category named '{dto.Name}' already exists.");

        // Check if a previously soft-deleted category with the same name exists
        var deletedEntity = await _context.Categories
            .Include(x => x.Listings)
            .FirstOrDefaultAsync(x => x.Name.ToLower() == trimmedName.ToLower() &&
                                      x.Status == RecordStatus.Deleted, cancellationToken);

        Category entity;

        if (deletedEntity is not null)
        {
            // ── RESTORE path: reuse the same row so all FK references are preserved ──
            entity = deletedEntity;
            entity.Description = dto.Description ?? entity.Description;
            entity.BookingType = dto.BookingType ?? entity.BookingType;
            entity.ServiceModel = dto.ServiceModel ?? entity.ServiceModel;
            entity.DateSelectionEnabled = dto.DateSelectionEnabled;
            entity.TimeSlotEnabled = dto.TimeSlotEnabled;
            entity.AvailabilityCalendarEnabled = dto.AvailabilityCalendarEnabled;
            entity.DefaultCommissionPercent = dto.DefaultCommissionPercent;
            entity.PlatformServiceFee = dto.PlatformServiceFee ?? entity.PlatformServiceFee;
            entity.TaxApplicable = dto.TaxApplicable;
            entity.Icon = dto.Icon ?? entity.Icon;
            entity.BannerImageUrl = dto.BannerImageUrl ?? entity.BannerImageUrl;
            entity.DisplayOrder = dto.DisplayOrder;
            entity.IsFeatured = dto.IsFeatured;
            entity.RequiresAdminApproval = dto.RequiresAdminApproval;
            entity.Status = ParseStatus(dto.Status);
            entity.UpdatedAtUtc = DateTime.UtcNow;
        }
        else
        {
            // ── CREATE path: brand new category ──
            entity = new Category
            {
                Name = trimmedName,
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
        }

        await _context.SaveChangesAsync(cancellationToken);

        // ── Re-link all orphaned listings that belong to this category ──────
        var uncategorized = await _context.Categories
            .FirstOrDefaultAsync(c => c.Name == CategoryConstants.UncategorizedName, cancellationToken);

        if (uncategorized is not null)
        {
            var orphaned = await _context.Listings
                .Where(l => l.CategoryId == uncategorized.Id
                         && l.OriginalCategoryName != null
                         && l.OriginalCategoryName.ToLower() == trimmedName.ToLower())
                .ToListAsync(cancellationToken);

            foreach (var listing in orphaned)
            {
                listing.CategoryId = entity.Id;
                listing.Status = RecordStatus.Active;
            }

            if (orphaned.Count > 0)
                await _context.SaveChangesAsync(cancellationToken);
        }

        var listingCount = await _context.Listings
            .CountAsync(l => l.CategoryId == entity.Id, cancellationToken);

        return ToDto(entity, listingCount);
    }

    // ── Update ─────────────────────────────────────────────────────────────
    public async Task<CategoryDto?> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken)
    {
        var entity = await _context.Categories
            .Include(x => x.Listings)
            .FirstOrDefaultAsync(x => x.Id == id && x.Status != RecordStatus.Deleted, cancellationToken);

        if (entity is null) return null;

        // If the category name is being changed, update OriginalCategoryName
        // on all its listings so re-linking still works if it's later deleted.
        if (dto.Name is not null && !dto.Name.Trim().Equals(entity.Name, StringComparison.OrdinalIgnoreCase))
        {
            var affectedListings = await _context.Listings
                .Where(l => l.CategoryId == entity.Id)
                .ToListAsync(cancellationToken);

            foreach (var listing in affectedListings)
                listing.OriginalCategoryName = dto.Name.Trim();

            entity.Name = dto.Name.Trim();
        }

        if (dto.Description is not null) entity.Description = dto.Description;
        if (dto.BookingType is not null) entity.BookingType = dto.BookingType;
        if (dto.ServiceModel is not null) entity.ServiceModel = dto.ServiceModel;
        if (dto.DateSelectionEnabled.HasValue) entity.DateSelectionEnabled = dto.DateSelectionEnabled.Value;
        if (dto.TimeSlotEnabled.HasValue) entity.TimeSlotEnabled = dto.TimeSlotEnabled.Value;
        if (dto.AvailabilityCalendarEnabled.HasValue) entity.AvailabilityCalendarEnabled = dto.AvailabilityCalendarEnabled.Value;
        if (dto.DefaultCommissionPercent.HasValue) entity.DefaultCommissionPercent = dto.DefaultCommissionPercent.Value;
        if (dto.PlatformServiceFee.HasValue) entity.PlatformServiceFee = dto.PlatformServiceFee.Value;
        if (dto.TaxApplicable.HasValue) entity.TaxApplicable = dto.TaxApplicable.Value;
        if (dto.Icon is not null) entity.Icon = dto.Icon;
        if (dto.BannerImageUrl is not null) entity.BannerImageUrl = dto.BannerImageUrl;
        if (dto.DisplayOrder.HasValue) entity.DisplayOrder = dto.DisplayOrder.Value;
        if (dto.IsFeatured.HasValue) entity.IsFeatured = dto.IsFeatured.Value;
        if (dto.RequiresAdminApproval.HasValue) entity.RequiresAdminApproval = dto.RequiresAdminApproval.Value;
        if (dto.Status is not null) entity.Status = ParseStatus(dto.Status, entity.Status);

        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(entity, entity.Listings.Count);
    }

    // ── Delete ─────────────────────────────────────────────────────────────
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _context.Categories
            .Include(x => x.Listings)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null) return false;

        if (entity.Listings.Any())
        {
            // Ensure the __Uncategorized__ holding category exists
            var uncategorized = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == CategoryConstants.UncategorizedName, cancellationToken);

            if (uncategorized is null)
            {
                uncategorized = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = CategoryConstants.UncategorizedName,
                    Status = RecordStatus.Inactive,
                    DisplayOrder = int.MaxValue,
                };
                _context.Categories.Add(uncategorized);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Park listings under __Uncategorized__ and stamp OriginalCategoryName
            // so they can be re-linked automatically when this category is re-created.
            foreach (var listing in entity.Listings)
            {
                if (string.IsNullOrEmpty(listing.OriginalCategoryName))
                    listing.OriginalCategoryName = entity.Name;

                listing.CategoryId = uncategorized.Id;
                listing.Status = RecordStatus.Inactive;
            }
        }

        // Soft-delete the category
        entity.Status = RecordStatus.Deleted;
        entity.UpdatedAtUtc = DateTime.UtcNow;

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