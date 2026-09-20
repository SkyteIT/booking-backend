using Moq;
using Ube.Application.Common.Exceptions;
using Ube.Application.Features.Content.Category;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums;
using Ube.Domain.Enums.Listings;
using Ube.Domain.Enums.Vendors;

namespace Ube.Tests.Vendors;

public class VendorListingCategoryServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Mock<IVendorApplicationRepository> _applications = new();
    private readonly Mock<IVendorProfileRepository> _profiles = new();
    private readonly Mock<ICategoryRepository> _categories = new();

    private VendorListingCategoryService Build(string? selected, VendorApplicationStatus status = VendorApplicationStatus.Approved)
    {
        _profiles.Setup(p => p.GetVendorIdAsync(_userId)).ReturnsAsync(new VendorProfile { IsActive = true });
        _applications.Setup(a => a.GetLatestByUserIdAsync(_userId)).ReturnsAsync(new VendorApplication { Categories = selected, Status = status });
        return new(_applications.Object, _profiles.Object, _categories.Object);
    }

    [Fact]
    public async Task ReturnsOnlyApprovedCategories_NotEveryCategoryOfTheSameType()
    {
        var hotel = new Category { Id = Guid.NewGuid(), Name = "Hotels", Type = ListingType.Hotel, Status = RecordStatus.Active };
        var luxury = new Category { Id = Guid.NewGuid(), Name = "Luxury Hotels", Type = ListingType.Hotel, Status = RecordStatus.Active };
        var activity = new Category { Id = Guid.NewGuid(), Name = "Activities", Type = ListingType.Activity, Status = RecordStatus.Active };
        _categories.Setup(c => c.GetAllAsync(default)).ReturnsAsync(new[] { hotel, luxury, activity });
        var service = Build($" hotels , {activity.Id}");

        var result = await service.GetAllowedAsync(_userId);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Id == hotel.Id);
        Assert.Contains(result, c => c.Id == activity.Id);
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.EnsureAllowedAsync(_userId, luxury.Id));
    }

    [Theory]
    [InlineData(VendorApplicationStatus.Pending)]
    [InlineData(VendorApplicationStatus.Rejected)]
    public async Task UnapprovedApplicationHasNoCategories(VendorApplicationStatus status)
    {
        Assert.Empty(await Build("Hotels", status).GetAllowedAsync(_userId));
        _categories.Verify(c => c.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MissingApplicationHasNoCategories()
    {
        var service = Build("Hotels");
        _applications.Setup(a => a.GetLatestByUserIdAsync(_userId)).ReturnsAsync((VendorApplication?)null);
        Assert.Empty(await service.GetAllowedAsync(_userId));
    }

    [Fact]
    public async Task InactiveUnconfiguredAndUnselectedCategoriesAreExcluded()
    {
        _categories.Setup(c => c.GetAllAsync(default)).ReturnsAsync(new[] {
            new Category { Id = Guid.NewGuid(), Name = "Hotels", Type = ListingType.Hotel, Status = RecordStatus.Inactive },
            new Category { Id = Guid.NewGuid(), Name = "Other", Type = null, Status = RecordStatus.Active }
        });
        Assert.Empty(await Build("Hotels,Other").GetAllowedAsync(_userId));
        Assert.Empty(await Build(null).GetAllowedAsync(_userId));
    }
}
