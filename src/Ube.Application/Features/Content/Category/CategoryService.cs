using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Constants;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Enums;

namespace Ube.Application.Features.Content.Category;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepo;
    private readonly IListingRepository _listingRepo;

    public CategoryService(ICategoryRepository categoryRepo, IListingRepository listingRepo)
    {
        _categoryRepo = categoryRepo;
        _listingRepo = listingRepo;
    }

    private static CategoryDto ToDto(Ube.Domain.Entities.Listings.Category x, int listingCount) => new()
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
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };

    private static RecordStatus ParseStatus(string? raw, RecordStatus fallback = RecordStatus.Active)
    {
        if (Enum.TryParse<RecordStatus>(raw, ignoreCase: true, out var parsed))
            return parsed;
        return fallback;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var categories = await _categoryRepo.GetAllAsync(cancellationToken);
        return categories.Select(x => ToDto(x, x.Listings.Count)).ToList();
    }

    public async Task<IReadOnlyList<CategoryDto>> GetFilteredAsync(
        string? status, string? search, CancellationToken cancellationToken)
    {
        RecordStatus? recordStatus = null;
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<RecordStatus>(status, ignoreCase: true, out var parsed))
            recordStatus = parsed;

        var categories = await _categoryRepo.GetFilteredAsync(recordStatus, search, cancellationToken);
        return categories.Select(x => ToDto(x, x.Listings.Count)).ToList();
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var x = await _categoryRepo.GetByIdAsync(id, includeListings: true, cancellationToken);
        return x is null ? null : ToDto(x, x.Listings.Count);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken)
    {
        var trimmedName = dto.Name.Trim();

        var activeExists = await _categoryRepo.ExistsByNameAsync(trimmedName, cancellationToken);
        if (activeExists)
            throw new InvalidOperationException($"A category named '{dto.Name}' already exists.");

        var deletedEntity = await _categoryRepo.GetDeletedByNameAsync(trimmedName, cancellationToken);

        Ube.Domain.Entities.Listings.Category entity;

        if (deletedEntity is not null)
        {
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
            entity.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            entity = new Ube.Domain.Entities.Listings.Category
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
            await _categoryRepo.AddAsync(entity, cancellationToken);
        }

        await _categoryRepo.SaveChangesAsync(cancellationToken);

        var uncategorized = await _categoryRepo.GetUncategorizedAsync(cancellationToken);
        if (uncategorized is not null)
        {
            var orphaned = await _listingRepo.GetOrphanedByCategoryNameAsync(uncategorized.Id, trimmedName, cancellationToken);
            foreach (var listing in orphaned)
            {
                listing.CategoryId = entity.Id;
                listing.IsActive = true;
            }

            if (orphaned.Count > 0)
                await _categoryRepo.SaveChangesAsync(cancellationToken);
        }

        var listingCount = await _categoryRepo.CountListingsAsync(entity.Id, cancellationToken);
        return ToDto(entity, listingCount);
    }

    public async Task<CategoryDto?> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken)
    {
        var entity = await _categoryRepo.GetByIdAsync(id, includeListings: true, cancellationToken);
        if (entity is null) return null;

        if (dto.Name is not null && !dto.Name.Trim().Equals(entity.Name, StringComparison.OrdinalIgnoreCase))
        {
            var affectedListings = await _listingRepo.GetByCategoryIdAsync(entity.Id, cancellationToken);
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

        entity.UpdatedAt = DateTime.UtcNow;
        await _categoryRepo.SaveChangesAsync(cancellationToken);

        return ToDto(entity, entity.Listings.Count);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _categoryRepo.GetByIdAsync(id, includeListings: true, cancellationToken);
        if (entity is null) return false;

        if (entity.Listings.Any())
        {
            var uncategorized = await _categoryRepo.GetUncategorizedAsync(cancellationToken);
            if (uncategorized is null)
            {
                uncategorized = new Ube.Domain.Entities.Listings.Category
                {
                    Id = Guid.NewGuid(),
                    Name = CategoryConstants.UncategorizedName,
                    Status = RecordStatus.Inactive,
                    DisplayOrder = int.MaxValue,
                };
                await _categoryRepo.AddAsync(uncategorized, cancellationToken);
                await _categoryRepo.SaveChangesAsync(cancellationToken);
            }

            foreach (var listing in entity.Listings)
            {
                if (string.IsNullOrEmpty(listing.OriginalCategoryName))
                    listing.OriginalCategoryName = entity.Name;

                listing.CategoryId = uncategorized.Id;
                listing.IsActive = false;
            }
        }

        entity.Status = RecordStatus.Deleted;
        entity.UpdatedAt = DateTime.UtcNow;

        await _categoryRepo.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<CategoryDto?> ToggleStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var entity = await _categoryRepo.GetByIdAsync(id, includeListings: true, cancellationToken);
        if (entity is null) return null;

        entity.Status = isActive ? RecordStatus.Active : RecordStatus.Inactive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _categoryRepo.SaveChangesAsync(cancellationToken);

        return ToDto(entity, entity.Listings.Count);
    }
}
