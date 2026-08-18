using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Enums.Listings;

namespace Ube.Application.Features.Listings;

public class ListingOfferService : IListingOfferService
{
    private readonly IListingOfferRepository _offerRepo;
    private readonly IListingRepository _listingRepo;
    private readonly IVendorProfileRepository _vendorProfileRepo;

    public ListingOfferService(
        IListingOfferRepository offerRepo,
        IListingRepository listingRepo,
        IVendorProfileRepository vendorProfileRepo)
    {
        _offerRepo = offerRepo;
        _listingRepo = listingRepo;
        _vendorProfileRepo = vendorProfileRepo;
    }

    public async Task<IReadOnlyList<ListingOfferDto>> GetForListingAsync(Guid listingId, CancellationToken ct = default)
    {
        var offers = await _offerRepo.GetByListingIdAsync(listingId, ct);
        return offers.Select(ToDto).ToList();
    }

    public async Task<ListingOfferDto> CreateAsync(Guid listingId, Guid userId, CreateListingOfferRequest request, CancellationToken ct = default)
    {
        await EnsureOwnedListingAsync(listingId, userId, ct);

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new BusinessRuleException("Title is required.");

        ValidateDateRange(request.StartDate, request.EndDate);
        ValidateDiscount(request.DiscountType, request.DiscountValue);

        if (request.DiscountType.HasValue)
        {
            var overlaps = await _offerRepo.HasActiveDiscountOverlapAsync(listingId, request.StartDate, request.EndDate, excludeOfferId: null, ct);
            if (overlaps)
                throw new BusinessRuleException("This listing already has an active discount offer overlapping these dates.");
        }

        var offer = new ListingOffer
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountType.HasValue ? request.DiscountValue : null,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        await _offerRepo.AddAsync(offer, ct);
        return ToDto(offer);
    }

    public async Task<ListingOfferDto> UpdateAsync(Guid listingId, Guid offerId, Guid userId, UpdateListingOfferRequest request, CancellationToken ct = default)
    {
        await EnsureOwnedListingAsync(listingId, userId, ct);

        var offer = await _offerRepo.GetByIdAsync(offerId, ct)
            ?? throw new NotFoundException("Offer not found");
        if (offer.ListingId != listingId)
            throw new NotFoundException("Offer not found");

        var newStart = request.StartDate ?? offer.StartDate;
        var newEnd = request.EndDate ?? offer.EndDate;
        ValidateDateRange(newStart, newEnd);

        var newDiscountType = request.ClearDiscount ? null : request.DiscountType ?? offer.DiscountType;
        var newDiscountValue = request.ClearDiscount ? null : request.DiscountValue ?? offer.DiscountValue;
        ValidateDiscount(newDiscountType, newDiscountValue);

        if (newDiscountType.HasValue)
        {
            var overlaps = await _offerRepo.HasActiveDiscountOverlapAsync(listingId, newStart, newEnd, excludeOfferId: offer.Id, ct);
            if (overlaps)
                throw new BusinessRuleException("This listing already has an active discount offer overlapping these dates.");
        }

        if (request.Title is not null) offer.Title = request.Title.Trim();
        if (request.Description is not null) offer.Description = request.Description.Trim();
        offer.DiscountType = newDiscountType;
        offer.DiscountValue = newDiscountType.HasValue ? newDiscountValue : null;
        offer.StartDate = newStart;
        offer.EndDate = newEnd;
        if (request.IsActive.HasValue) offer.IsActive = request.IsActive.Value;
        offer.UpdatedAt = DateTime.UtcNow;

        await _offerRepo.UpdateAsync(offer, ct);
        return ToDto(offer);
    }

    public async Task DeleteAsync(Guid listingId, Guid offerId, Guid userId, CancellationToken ct = default)
    {
        await EnsureOwnedListingAsync(listingId, userId, ct);

        var offer = await _offerRepo.GetByIdAsync(offerId, ct)
            ?? throw new NotFoundException("Offer not found");
        if (offer.ListingId != listingId)
            throw new NotFoundException("Offer not found");

        await _offerRepo.DeleteAsync(offer, ct);
    }

    private static void ValidateDateRange(DateOnly start, DateOnly end)
    {
        if (end < start)
            throw new BusinessRuleException("End date must be on or after the start date.");
    }

    private static void ValidateDiscount(OfferDiscountType? type, decimal? value)
    {
        if (!type.HasValue) return;
        if (!value.HasValue || value.Value <= 0)
            throw new BusinessRuleException("A discount offer requires a positive discount value.");
        if (type == OfferDiscountType.PercentageDiscount && value.Value > 100)
            throw new BusinessRuleException("A percentage discount cannot exceed 100.");
    }

    private async Task EnsureOwnedListingAsync(Guid listingId, Guid userId, CancellationToken ct)
    {
        var listing = await _listingRepo.GetByIdAsync(listingId)
            ?? throw new NotFoundException("Listing not found");

        var vendorProfile = await _vendorProfileRepo.GetVendorIdAsync(userId);
        if (vendorProfile == null || vendorProfile.Id != listing.VendorProfileId)
            throw new ForbiddenException("You do not have permission to manage offers for this listing.");
    }

    private static ListingOfferDto ToDto(ListingOffer o) => new()
    {
        Id = o.Id,
        ListingId = o.ListingId,
        Title = o.Title,
        Description = o.Description,
        DiscountType = o.DiscountType,
        DiscountValue = o.DiscountValue,
        StartDate = o.StartDate,
        EndDate = o.EndDate,
        IsActive = o.IsActive
    };
}
