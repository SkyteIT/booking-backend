using Moq;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Content.Category;
using Ube.Application.Features.Listings;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Listings;

namespace Ube.Tests.Listings;

public class SeasonalPricingServiceTests
{
    private sealed record Ctx(
        Mock<ISeasonalPricingRepository> RuleRepo,
        Mock<IListingRepository> ListingRepo,
        Mock<IListingUnitRepository> UnitRepo,
        Mock<ICategoryRepository> CategoryRepo,
        Mock<IVendorProfileRepository> VendorProfileRepo,
        SeasonalPricingService Service);

    private static readonly Guid VendorUserId = Guid.NewGuid();
    private static readonly Guid VendorProfileId = Guid.NewGuid();

    private static Ctx Build(Listing listing, Category category)
    {
        var ruleRepo = new Mock<ISeasonalPricingRepository>();
        var listingRepo = new Mock<IListingRepository>();
        var unitRepo = new Mock<IListingUnitRepository>();
        var categoryRepo = new Mock<ICategoryRepository>();
        var vendorProfileRepo = new Mock<IVendorProfileRepository>();

        listingRepo.Setup(r => r.GetByIdAsync(listing.Id)).ReturnsAsync(listing);
        categoryRepo.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(category);
        vendorProfileRepo.Setup(r => r.GetVendorIdAsync(VendorUserId)).ReturnsAsync(new VendorProfile { Id = VendorProfileId });
        ruleRepo.Setup(r => r.HasOverlapAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = new SeasonalPricingService(ruleRepo.Object, listingRepo.Object, unitRepo.Object, categoryRepo.Object, vendorProfileRepo.Object);
        return new Ctx(ruleRepo, listingRepo, unitRepo, categoryRepo, vendorProfileRepo, service);
    }

    private static Listing MakeListing(Guid categoryId, Guid vendorProfileId) => new()
    {
        Id = Guid.NewGuid(),
        Title = "Beach Resort",
        Price = 1000m,
        Currency = "LKR",
        CategoryId = categoryId,
        VendorProfileId = vendorProfileId
    };

    private static Category MakePerNightCategory() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Hotels",
        ServiceModel = PricingUnit.PerNight
    };

    private static CreateSeasonalPricingRuleRequest MakeRequest(DateOnly start, DateOnly end) => new()
    {
        Name = "Peak Season",
        StartDate = start,
        EndDate = end,
        AdjustmentType = SeasonalRateAdjustmentType.PercentageAdjustment,
        AdjustmentValue = 30
    };

    [Fact]
    public async Task CreateAsync_Succeeds_For_Valid_NonOverlapping_Rule()
    {
        var category = MakePerNightCategory();
        var listing = MakeListing(category.Id, VendorProfileId);
        var ctx = Build(listing, category);

        var dto = await ctx.Service.CreateAsync(listing.Id, VendorUserId, MakeRequest(new DateOnly(2026, 12, 20), new DateOnly(2027, 1, 5)));

        Assert.Equal("Peak Season", dto.Name);
        ctx.RuleRepo.Verify(r => r.AddAsync(It.IsAny<SeasonalPricingRule>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Rejects_Overlapping_Range()
    {
        var category = MakePerNightCategory();
        var listing = MakeListing(category.Id, VendorProfileId);
        var ctx = Build(listing, category);
        ctx.RuleRepo.Setup(r => r.HasOverlapAsync(listing.Id, null, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ctx.Service.CreateAsync(listing.Id, VendorUserId, MakeRequest(new DateOnly(2026, 12, 20), new DateOnly(2027, 1, 5))));
    }

    [Fact]
    public async Task CreateAsync_Rejects_EndDate_Before_StartDate()
    {
        var category = MakePerNightCategory();
        var listing = MakeListing(category.Id, VendorProfileId);
        var ctx = Build(listing, category);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ctx.Service.CreateAsync(listing.Id, VendorUserId, MakeRequest(new DateOnly(2027, 1, 5), new DateOnly(2026, 12, 20))));
    }

    [Fact]
    public async Task CreateAsync_Rejects_When_Category_Is_Not_Date_Based()
    {
        var category = new Category { Id = Guid.NewGuid(), Name = "Tickets", ServiceModel = PricingUnit.FixedPrice };
        var listing = MakeListing(category.Id, VendorProfileId);
        var ctx = Build(listing, category);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ctx.Service.CreateAsync(listing.Id, VendorUserId, MakeRequest(new DateOnly(2026, 12, 20), new DateOnly(2027, 1, 5))));
    }

    [Fact]
    public async Task CreateAsync_Rejects_NonOwner_Vendor()
    {
        var category = MakePerNightCategory();
        var listing = MakeListing(category.Id, VendorProfileId);
        var ctx = Build(listing, category);
        var otherUserId = Guid.NewGuid();
        ctx.VendorProfileRepo.Setup(r => r.GetVendorIdAsync(otherUserId)).ReturnsAsync(new VendorProfile { Id = Guid.NewGuid() });

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            ctx.Service.CreateAsync(listing.Id, otherUserId, MakeRequest(new DateOnly(2026, 12, 20), new DateOnly(2027, 1, 5))));
    }

    [Fact]
    public async Task GetPriceQuoteAsync_Applies_Seasonal_Rules_For_PerNight_Category()
    {
        var category = MakePerNightCategory();
        var listing = MakeListing(category.Id, VendorProfileId);
        var ctx = Build(listing, category);

        var start = new DateTime(2026, 12, 20);
        var end = start.AddDays(2);
        var rule = new SeasonalPricingRule
        {
            Id = Guid.NewGuid(),
            StartDate = DateOnly.FromDateTime(start),
            EndDate = DateOnly.FromDateTime(end),
            AdjustmentType = SeasonalRateAdjustmentType.PercentageAdjustment,
            AdjustmentValue = 50,
            IsActive = true
        };
        ctx.RuleRepo.Setup(r => r.GetActiveInRangeAsync(listing.Id, null, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SeasonalPricingRule> { rule });

        var quote = await ctx.Service.GetPriceQuoteAsync(listing.Id, null, start, end, 1);

        Assert.Equal(3000m, quote.TotalAmount); // 2 nights * 1500 (1000 + 50%)
    }

    [Fact]
    public async Task GetPriceQuoteAsync_Uses_Flat_Calculation_For_NonDateBased_Category()
    {
        var category = new Category { Id = Guid.NewGuid(), Name = "Tickets", ServiceModel = PricingUnit.FixedPrice };
        var listing = MakeListing(category.Id, VendorProfileId);
        var ctx = Build(listing, category);

        var quote = await ctx.Service.GetPriceQuoteAsync(listing.Id, null, new DateTime(2026, 12, 20), new DateTime(2026, 12, 25), 3);

        Assert.Equal(3000m, quote.TotalAmount); // 1000 * 3, duration irrelevant for FixedPrice
        ctx.RuleRepo.Verify(r => r.GetActiveInRangeAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
