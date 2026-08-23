using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Enums.Listings;

namespace Ube.Application.Features.Listings;

public class ListingUnitService : IListingUnitService
{
    private readonly IListingUnitRepository _unitRepo;
    private readonly IListingRepository _listingRepo;
    private readonly IVendorProfileRepository _vendorProfileRepo;
    private readonly IBookingRepository _bookingRepo;

    public ListingUnitService(
        IListingUnitRepository unitRepo,
        IListingRepository listingRepo,
        IVendorProfileRepository vendorProfileRepo,
        IBookingRepository bookingRepo)
    {
        _unitRepo = unitRepo;
        _listingRepo = listingRepo;
        _vendorProfileRepo = vendorProfileRepo;
        _bookingRepo = bookingRepo;
    }

    public async Task<IReadOnlyList<Guid>> GetBookedUnitIdsAsync(Guid listingId, DateTime start, DateTime end, CancellationToken ct = default)
        => await _bookingRepo.GetBookedListingUnitIdsAsync(listingId, start, end, ct);

    public async Task<IReadOnlyList<ListingUnitDto>> GetForListingAsync(Guid listingId, CancellationToken ct = default)
    {
        var units = await _unitRepo.GetByListingIdAsync(listingId, ct);
        return units.Select(ToDto).ToList();
    }

    public async Task<ListingUnitCleanupResult> CleanupDuplicatesAsync(CancellationToken ct = default)
        => await _unitRepo.CleanupDuplicatesAsync(ct);

    public async Task<ListingUnitDto> AddAsync(Guid listingId, Guid userId, AddListingUnitRequest request, CancellationToken ct = default)
    {
        var listing = await EnsureOwnedListingAsync(listingId, userId, ct);

        var unit = new ListingUnit
        {
            Id = Guid.NewGuid(),
            ListingId = listing.Id,
            Kind = ListingUnitKind.Generic,
            Name = request.Name,
            Description = request.Description,
            PriceOverride = request.PriceOverride,
            Capacity = request.Capacity,
            DisplayOrder = request.DisplayOrder
        };

        await _unitRepo.AddAsync(unit, ct);
        return ToDto(unit);
    }

    public async Task<IReadOnlyList<ListingUnitDto>> AddGridAsync(Guid listingId, Guid userId, AddListingUnitsGridRequest request, CancellationToken ct = default)
    {
        await EnsureOwnedListingAsync(listingId, userId, ct);

        if (request.Rows <= 0 || request.Columns <= 0)
            throw new BusinessRuleException("Rows and columns must both be positive.");

        // Re-running grid generation (a retry after a dropped response, a
        // double-click, or regenerating with different dimensions) must
        // replace the previous seat map, not layer a second copy on top of
        // it with fresh ids.
        await _unitRepo.DeleteByListingAndKindAsync(listingId, ListingUnitKind.Seat, ct);

        var units = new List<ListingUnit>();
        for (var row = 0; row < request.Rows; row++)
        {
            var rowLabel = request.RowLabels != null && row < request.RowLabels.Count
                ? request.RowLabels[row]
                : ((char)('A' + row % 26)).ToString();

            for (var col = 0; col < request.Columns; col++)
            {
                var code = $"{rowLabel}{col + 1}";
                units.Add(new ListingUnit
                {
                    Id = Guid.NewGuid(),
                    ListingId = listingId,
                    Kind = ListingUnitKind.Seat,
                    Name = $"Seat {code}",
                    Code = code,
                    PriceOverride = request.PricePerSeat,
                    Capacity = 1,
                    RowIndex = row,
                    ColumnIndex = col,
                    DisplayOrder = row * request.Columns + col
                });
            }
        }

        await _unitRepo.AddRangeAsync(units, ct);
        return units.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<ListingUnitDto>> AddTimeSlotsAsync(Guid listingId, Guid userId, AddListingUnitsTimeSlotsRequest request, CancellationToken ct = default)
    {
        await EnsureOwnedListingAsync(listingId, userId, ct);

        if (request.SlotDurationMinutes <= 0)
            throw new BusinessRuleException("Slot duration must be positive.");
        if (request.EndTime <= request.StartTime)
            throw new BusinessRuleException("End time must be after start time.");

        // Same replace-not-append reasoning as AddGridAsync.
        await _unitRepo.DeleteByListingAndKindAsync(listingId, ListingUnitKind.TimeSlot, ct);

        var duration = TimeSpan.FromMinutes(request.SlotDurationMinutes);
        var units = new List<ListingUnit>();
        var order = 0;
        for (var slotStart = request.StartTime; slotStart + duration <= request.EndTime; slotStart += duration)
        {
            units.Add(new ListingUnit
            {
                Id = Guid.NewGuid(),
                ListingId = listingId,
                Kind = ListingUnitKind.TimeSlot,
                Name = slotStart.ToString(@"hh\:mm"),
                PriceOverride = request.Price,
                Capacity = request.CapacityPerSlot,
                SlotStartTime = slotStart,
                SlotDuration = duration,
                DisplayOrder = order++
            });
        }

        if (units.Count == 0)
            throw new BusinessRuleException("The given time range produces no slots — check start/end time and slot duration.");

        await _unitRepo.AddRangeAsync(units, ct);
        return units.Select(ToDto).ToList();
    }

    public async Task<ListingUnitDto> UpdateAsync(Guid listingId, Guid unitId, Guid userId, UpdateListingUnitRequest request, CancellationToken ct = default)
    {
        await EnsureOwnedListingAsync(listingId, userId, ct);

        var unit = await _unitRepo.GetByIdAsync(unitId, ct)
            ?? throw new NotFoundException("Listing unit not found");
        if (unit.ListingId != listingId)
            throw new NotFoundException("Listing unit not found");

        if (request.Name is not null) unit.Name = request.Name;
        if (request.Description is not null) unit.Description = request.Description;
        if (request.PriceOverride.HasValue) unit.PriceOverride = request.PriceOverride;
        if (request.Capacity.HasValue) unit.Capacity = request.Capacity.Value;
        if (request.IsActive.HasValue) unit.IsActive = request.IsActive.Value;
        if (request.DisplayOrder.HasValue) unit.DisplayOrder = request.DisplayOrder.Value;

        await _unitRepo.UpdateAsync(unit, ct);
        return ToDto(unit);
    }

    public async Task DeleteAsync(Guid listingId, Guid unitId, Guid userId, CancellationToken ct = default)
    {
        await EnsureOwnedListingAsync(listingId, userId, ct);

        var unit = await _unitRepo.GetByIdAsync(unitId, ct)
            ?? throw new NotFoundException("Listing unit not found");
        if (unit.ListingId != listingId)
            throw new NotFoundException("Listing unit not found");

        await _unitRepo.DeleteAsync(unit, ct);
    }

    private async Task<Listing> EnsureOwnedListingAsync(Guid listingId, Guid userId, CancellationToken ct)
    {
        var listing = await _listingRepo.GetByIdAsync(listingId)
            ?? throw new NotFoundException("Listing not found");

        var vendorProfile = await _vendorProfileRepo.GetVendorIdAsync(userId);
        if (vendorProfile == null || vendorProfile.Id != listing.VendorProfileId)
            throw new ForbiddenException("You do not have permission to manage units for this listing.");

        return listing;
    }

    private static ListingUnitDto ToDto(ListingUnit u) => new()
    {
        Id = u.Id,
        ListingId = u.ListingId,
        Kind = u.Kind,
        Name = u.Name,
        Code = u.Code,
        Description = u.Description,
        PriceOverride = u.PriceOverride,
        Capacity = u.Capacity,
        RowIndex = u.RowIndex,
        ColumnIndex = u.ColumnIndex,
        SlotStartTime = u.SlotStartTime,
        SlotDuration = u.SlotDuration,
        IsActive = u.IsActive,
        DisplayOrder = u.DisplayOrder
    };
}
