using Moq;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Listings;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Listings;

namespace Ube.Tests.Listings;

public class ListingOfferServiceTests
{
    private sealed record Ctx(
        Mock<IListingOfferRepository> OfferRepo,
        Mock<IListingRepository> ListingRepo,
        Mock<IVendorProfileRepository> VendorProfileRepo,
        ListingOfferService Service);

    private static readonly Guid VendorUserId = Guid.NewGuid();
    private static readonly Guid VendorProfileId = Guid.NewGuid();

    private static Ctx Build(Listing listing)
    {
        var offerRepo = new Mock<IListingOfferRepository>();
        var listingRepo = new Mock<IListingRepository>();
        var vendorProfileRepo = new Mock<IVendorProfileRepository>();

        listingRepo.Setup(r => r.GetByIdAsync(listing.Id)).ReturnsAsync(listing);
        vendorProfileRepo.Setup(r => r.GetVendorIdAsync(VendorUserId)).ReturnsAsync(new VendorProfile { Id = VendorProfileId });
        offerRepo.Setup(r => r.HasActiveDiscountOverlapAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = new ListingOfferService(offerRepo.Object, listingRepo.Object, vendorProfileRepo.Object);
        return new Ctx(offerRepo, listingRepo, vendorProfileRepo, service);
    }

    private static Listing MakeListing() => new()
    {
        Id = Guid.NewGuid(),
        Title = "Beach Resort",
        Price = 1000m,
        Currency = "LKR",
        VendorProfileId = VendorProfileId
    };

    private static CreateListingOfferRequest MakeRequest(
        OfferDiscountType? discountType = null, decimal? discountValue = null,
        DateOnly? start = null, DateOnly? end = null) => new()
    {
        Title = "Free Breakfast",
        Description = "Free breakfast included with every booking.",
        DiscountType = discountType,
        DiscountValue = discountValue,
        StartDate = start ?? new DateOnly(2026, 9, 1),
        EndDate = end ?? new DateOnly(2026, 9, 30)
    };

    [Fact]
    public async Task CreateAsync_Allows_Pure_Perk_Offer_With_No_Discount()
    {
        var listing = MakeListing();
        var ctx = Build(listing);

        var dto = await ctx.Service.CreateAsync(listing.Id, VendorUserId, MakeRequest());

        Assert.Equal("Free Breakfast", dto.Title);
        Assert.Null(dto.DiscountType);
        ctx.OfferRepo.Verify(r => r.AddAsync(It.IsAny<ListingOffer>(), It.IsAny<CancellationToken>()), Times.Once);
        // A perk-only offer never needs an overlap check - it can't conflict on price.
        ctx.OfferRepo.Verify(r => r.HasActiveDiscountOverlapAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_Succeeds_For_Valid_Discount_Offer()
    {
        var listing = MakeListing();
        var ctx = Build(listing);

        var dto = await ctx.Service.CreateAsync(listing.Id, VendorUserId, MakeRequest(OfferDiscountType.PercentageDiscount, 15));

        Assert.Equal(OfferDiscountType.PercentageDiscount, dto.DiscountType);
        Assert.Equal(15, dto.DiscountValue);
    }

    [Fact]
    public async Task CreateAsync_Rejects_Overlapping_Discount_Offer()
    {
        var listing = MakeListing();
        var ctx = Build(listing);
        ctx.OfferRepo.Setup(r => r.HasActiveDiscountOverlapAsync(listing.Id, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ctx.Service.CreateAsync(listing.Id, VendorUserId, MakeRequest(OfferDiscountType.PercentageDiscount, 15)));
    }

    [Fact]
    public async Task CreateAsync_Rejects_Discount_Type_Without_Value()
    {
        var listing = MakeListing();
        var ctx = Build(listing);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ctx.Service.CreateAsync(listing.Id, VendorUserId, MakeRequest(OfferDiscountType.PercentageDiscount, null)));
    }

    [Fact]
    public async Task CreateAsync_Rejects_Percentage_Over_100()
    {
        var listing = MakeListing();
        var ctx = Build(listing);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ctx.Service.CreateAsync(listing.Id, VendorUserId, MakeRequest(OfferDiscountType.PercentageDiscount, 150)));
    }

    [Fact]
    public async Task CreateAsync_Rejects_EndDate_Before_StartDate()
    {
        var listing = MakeListing();
        var ctx = Build(listing);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ctx.Service.CreateAsync(listing.Id, VendorUserId, MakeRequest(start: new DateOnly(2026, 9, 30), end: new DateOnly(2026, 9, 1))));
    }

    [Fact]
    public async Task CreateAsync_Rejects_NonOwner_Vendor()
    {
        var listing = MakeListing();
        var ctx = Build(listing);
        var otherUserId = Guid.NewGuid();
        ctx.VendorProfileRepo.Setup(r => r.GetVendorIdAsync(otherUserId)).ReturnsAsync(new VendorProfile { Id = Guid.NewGuid() });

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            ctx.Service.CreateAsync(listing.Id, otherUserId, MakeRequest()));
    }
}
