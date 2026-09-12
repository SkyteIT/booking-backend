using Ube.Application.Common.Exceptions;
using Ube.Application.Features.Content.Category;
using Ube.Domain.Enums;
using Ube.Domain.Enums.Vendors;

namespace Ube.Application.Features.Vendors;

public interface IVendorListingCategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllowedAsync(Guid userId, CancellationToken ct = default);
    Task EnsureAllowedAsync(Guid userId, Guid categoryId, CancellationToken ct = default);
}

public class VendorListingCategoryService(
    IVendorApplicationRepository applications,
    IVendorProfileRepository profiles,
    ICategoryRepository categories) : IVendorListingCategoryService
{
    public async Task<IReadOnlyList<CategoryDto>> GetAllowedAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await profiles.GetVendorIdAsync(userId);
        if (profile == null || !profile.IsActive) return [];

        var application = await applications.GetLatestByUserIdAsync(userId);
        if (application?.Status != VendorApplicationStatus.Approved) return [];

        // Existing applications contain CSV category names. Also accept exact
        // category IDs; never grant all categories sharing the same listing type.
        var selected = (application.Categories ?? "")
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var available = await categories.GetAllAsync(ct);
        return available
            .Where(c => c.Status == RecordStatus.Active && c.Type.HasValue &&
                (selected.Contains(c.Id.ToString()) || selected.Contains(c.Name.Trim())))
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id, Name = c.Name, Description = c.Description, Type = c.Type,
                Status = c.Status.ToString(), ServiceModel = c.ServiceModel
            }).ToList();
    }

    public async Task EnsureAllowedAsync(Guid userId, Guid categoryId, CancellationToken ct = default)
    {
        var allowed = await GetAllowedAsync(userId, ct);
        if (!allowed.Any(c => c.Id == categoryId))
            throw new BusinessRuleException("You can only publish listings in categories selected in your approved vendor application. Contact support if your approved categories are missing.");
    }
}
