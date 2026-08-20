using System.Text.Json;
using Ube.Application.Common.Interfaces.Services;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Exceptions;
using Ube.Application.Features.Content.Category;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Listings;

namespace Ube.Application.Features.Listings;

public class ListingService : IListingService
{
    private readonly IListingRepository _listingRepository;
    private readonly IVendorProfileRepository _vendorProfileRepository;

    public ListingService(
        IListingRepository listingRepository,
        IVendorProfileRepository vendorProfileRepository)
    {
        _listingRepository = listingRepository;
        _vendorProfileRepository = vendorProfileRepository;
    }

    // ── Create ────────────────────────────────────────────────────────────────

    public async Task<Guid> CreateListingAsync(
        Guid userId,
        CreateListingRequest request,
        CancellationToken ct = default)
    {
        var vendor = await _vendorProfileRepository.GetVendorIdAsync(userId)
            ?? throw new BusinessRuleException("Vendor profile not found for this user.");

        var listing = new Listing
        {
            Id                 = Guid.NewGuid(),
            VendorProfileId    = vendor.Id,
            CategoryId         = request.CategoryId,
            Title              = request.Title,
            Description        = request.Description ?? string.Empty,
            Price              = request.Price,
            Currency           = request.Currency,
            Location           = request.Location,
            IsActive           = request.IsActive,
            Tags               = request.Tags.Count > 0 ? string.Join(",", request.Tags) : null,
            CancellationPolicy = request.CancellationPolicy,
            CreatedAt          = DateTime.UtcNow,
        };

        await _listingRepository.AddAsync(listing, ct);

        if (request.Images.Count > 0)
            await _listingRepository.ReplaceImagesAsync(listing.Id, request.Images, ct);

        if (request.CustomFieldValues.Count > 0)
        {
            var cfvs = request.CustomFieldValues.Select(v => new ListingCustomFieldValue
            {
                Id                    = Guid.NewGuid(),
                ListingId             = listing.Id,
                CategoryCustomFieldId = v.CategoryCustomFieldId,
                Value                 = v.Value,
            });
            await _listingRepository.ReplaceCustomFieldValuesAsync(listing.Id, cfvs, ct);
        }

        await UpsertDetailsFromCreateAsync(listing.Id, request, ct);

        return listing.Id;
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public async Task UpdateListingAsync(
        Guid listingId,
        Guid userId,
        UpdateListingRequest request,
        CancellationToken ct = default)
    {
        var vendor = await _vendorProfileRepository.GetVendorIdAsync(userId)
            ?? throw new BusinessRuleException("Vendor profile not found for this user.");

        var listing = await _listingRepository.GetByIdAsync(listingId)
            ?? throw new NotFoundException("Listing not found.");

        if (listing.VendorProfileId != vendor.Id)
            throw new BusinessRuleException("You do not own this listing.");

        listing.CategoryId         = request.CategoryId;
        listing.Title              = request.Title;
        listing.Description        = request.Description ?? string.Empty;
        listing.Price              = request.Price;
        listing.Currency           = request.Currency;
        listing.Location           = request.Location;
        listing.IsActive           = request.IsActive;
        listing.Tags               = request.Tags.Count > 0 ? string.Join(",", request.Tags) : null;
        listing.CancellationPolicy = request.CancellationPolicy;
        listing.UpdatedAt          = DateTime.UtcNow;

        await _listingRepository.UpdateAsync(listing);
        await _listingRepository.ReplaceImagesAsync(listing.Id, request.Images, ct);

        var cfvs = request.CustomFieldValues.Select(v => new ListingCustomFieldValue
        {
            Id                    = Guid.NewGuid(),
            ListingId             = listing.Id,
            CategoryCustomFieldId = v.CategoryCustomFieldId,
            Value                 = v.Value,
        });
        await _listingRepository.ReplaceCustomFieldValuesAsync(listing.Id, cfvs, ct);

        await UpsertDetailsFromUpdateAsync(listing.Id, request, ct);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    public async Task DeleteListingAsync(
        Guid listingId,
        Guid userId,
        CancellationToken ct = default)
    {
        var vendor = await _vendorProfileRepository.GetVendorIdAsync(userId)
            ?? throw new BusinessRuleException("Vendor profile not found for this user.");

        var listing = await _listingRepository.GetByIdAsync(listingId)
            ?? throw new NotFoundException("Listing not found.");

        if (listing.VendorProfileId != vendor.Id)
            throw new BusinessRuleException("You do not own this listing.");

        await _listingRepository.DeleteAsync(listing, ct);
    }

    // ── Read (single) ─────────────────────────────────────────────────────────

    public async Task<ListingResponse?> GetListingByIdAsync(
        Guid listingId,
        CancellationToken ct = default)
    {
        var listing = await _listingRepository.GetByIdWithDetailsAsync(listingId, ct);
        return listing == null ? null : MapToResponse(listing);
    }

    // ── Read (all) ────────────────────────────────────────────────────────────

    public async Task<List<ListingResponse>> GetAllListingsAsync(
        CancellationToken ct = default)
    {
        var listings = await _listingRepository.GetAllWithDetailsAsync(ct);
        return listings.Select(MapToResponse).ToList();
    }

    // ── Read (mine) ───────────────────────────────────────────────────────────

    public async Task<List<ListingResponse>> GetMyListingsAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var vendor = await _vendorProfileRepository.GetVendorIdAsync(userId)
            ?? throw new BusinessRuleException("Vendor profile not found for this user.");

        var listings = await _listingRepository
            .GetByVendorProfileIdWithDetailsAsync(vendor.Id, ct);

        return listings.Select(MapToResponse).ToList();
    }

    // ── Mapper ────────────────────────────────────────────────────────────────

    private static ListingResponse MapToResponse(Listing l) => new()
    {
        Id = l.Id,
        VendorProfileId = l.VendorProfileId,
        CategoryId = l.CategoryId,
        Title = l.Title,
        Description = l.Description,
        Price = l.Price,
        Currency = l.Currency,
        Location = l.Location,
        IsActive = l.IsActive,
        CategoryName = l.Category?.Name ?? string.Empty,
        VendorName = l.VendorProfile?.BusinessName ?? string.Empty,
        Type = l.Type,
        AverageRating = l.AverageRating,
        TotalReviews = l.TotalReviews,
        PrimaryImage = l.Images?.OrderByDescending(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault(),
        Images = l.Images?.Select(i => i.ImageUrl).ToList() ?? new List<string>(),
        Tags = l.Tags != null ? l.Tags.Split(", ", StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>(),
        CancellationPolicy = l.CancellationPolicy,
        HotelDetails = l.HotelDetails == null ? null : new HotelDetailsDto
        {
            PricePerNight   = l.HotelDetails.PricePerNight,
            AvailableRooms  = l.HotelDetails.AvailableRooms,
            Amenities       = SplitCsv(l.HotelDetails.Amenities),
            RoomTypes       = SplitCsv(l.HotelDetails.RoomTypes),
            CheckInTime     = l.HotelDetails.CheckInTime,
            CheckOutTime    = l.HotelDetails.CheckOutTime,
            PropertyType    = l.HotelDetails.PropertyType,
            PrimaryRoomType = l.HotelDetails.PrimaryRoomType,
        },

        RestaurantDetails = l.RestaurantDetails == null ? null : new RestaurantDetailsDto
        {
            CuisineType      = l.RestaurantDetails.CuisineType,
            AverageCost      = l.RestaurantDetails.AverageCost,
            OpeningHours     = l.RestaurantDetails.OpeningHours,
            TableCapacity    = l.RestaurantDetails.TableCapacity,
            TableTypes       = SplitCsv(l.RestaurantDetails.TableTypes),
            ReservationRules = l.RestaurantDetails.ReservationRules,
        },

        EventDetails = l.EventDetails == null ? null : new EventDetailsDto
        {
            EventName    = l.EventDetails.EventName,
            Organizer    = l.EventDetails.Organizer,
            DateAndTime  = l.EventDetails.DateAndTime,
            SeatCount    = l.EventDetails.SeatCount,
            TicketPrice  = l.EventDetails.TicketPrice,
            EventType    = l.EventDetails.EventType,
            VenueName    = l.EventDetails.VenueName,
            VenueAddress = l.EventDetails.VenueAddress,
            TicketTypes  = DeserializeJson<List<TicketTypeDto>>(l.EventDetails.TicketTypesJson),
        },

        CarRentalDetails = l.CarRentalDetails == null ? null : new CarRentalDetailsDto
        {
            Brand              = l.CarRentalDetails.Brand,
            Model              = l.CarRentalDetails.Model,
            Transmission       = l.CarRentalDetails.Transmission,
            PricePerDay        = l.CarRentalDetails.PricePerDay,
            SeatCount          = l.CarRentalDetails.SeatCount,
            FuelType           = l.CarRentalDetails.FuelType,
            AvailabilityStatus = l.CarRentalDetails.AvailabilityStatus,
            Year               = l.CarRentalDetails.Year,
            HourlyRate         = l.CarRentalDetails.HourlyRate,
            PickupLocation     = l.CarRentalDetails.PickupLocation,
            ReturnLocation     = l.CarRentalDetails.ReturnLocation,
            InsuranceOptions   = l.CarRentalDetails.InsuranceOptions,
        },

        ActivityDetails = l.ActivityDetails == null ? null : new ActivityDetailsDto
        {
            ActivityType = l.ActivityDetails.ActivityType,
            DurationHours = l.ActivityDetails.DurationHours,
            DifficultyLevel = l.ActivityDetails.DifficultyLevel,
            Price = l.ActivityDetails.Price,
            MinGroupSize = l.ActivityDetails.MinGroupSize,
            MaxGroupSize = l.ActivityDetails.MaxGroupSize,
            MinAge = l.ActivityDetails.MinAge,
            MaxAge = l.ActivityDetails.MaxAge,
            IncludedServices = l.ActivityDetails.IncludedServices?.Split(", ", StringSplitOptions.RemoveEmptyEntries).ToList(),
            SafetyRequirements = l.ActivityDetails.SafetyRequirements,
            AvailabilitySchedule = l.ActivityDetails.AvailabilitySchedule
        },
        CustomFieldValues = l.CustomFieldValues?.Select(v => new ListingCustomFieldValueDto
        {
            CategoryCustomFieldId = v.CategoryCustomFieldId,
            Label = v.CategoryCustomField.Label,
            Value = v.Value
        }).ToList() ?? new List<ListingCustomFieldValueDto>()
    };

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static List<string> SplitCsv(string? s)
        => string.IsNullOrWhiteSpace(s)
           ? new List<string>()
           : s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
              .ToList();

    private static T? DeserializeJson<T>(string? json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<T>(json); }
        catch { return null; }
    }

    // ── Detail upserts (Create) ───────────────────────────────────────────────

    private async Task UpsertDetailsFromCreateAsync(
        Guid listingId, CreateListingRequest r, CancellationToken ct)
    {
        if (r.HotelDetails != null)
            await _listingRepository.UpsertDetailsAsync(listingId, new HotelListingDetails
            {
                ListingId       = listingId,
                PricePerNight   = r.HotelDetails.PricePerNight,
                AvailableRooms  = r.HotelDetails.AvailableRooms,
                Amenities       = string.Join(",", r.HotelDetails.Amenities),
                RoomTypes       = string.Join(",", r.HotelDetails.RoomTypes),
                CheckInTime     = r.HotelDetails.CheckInTime,
                CheckOutTime    = r.HotelDetails.CheckOutTime,
                PropertyType    = r.HotelDetails.PropertyType,
                PrimaryRoomType = r.HotelDetails.PrimaryRoomType,
            }, ct);

        if (r.RestaurantDetails != null)
            await _listingRepository.UpsertDetailsAsync(listingId, new RestaurantListingDetails
            {
                ListingId        = listingId,
                CuisineType      = r.RestaurantDetails.CuisineType,
                AverageCost      = r.RestaurantDetails.AverageCost,
                OpeningHours     = r.RestaurantDetails.OpeningHours,
                TableCapacity    = r.RestaurantDetails.TableCapacity,
                TableTypes       = r.RestaurantDetails.TableTypes != null
                                   ? string.Join(",", r.RestaurantDetails.TableTypes)
                                   : null,
                ReservationRules = r.RestaurantDetails.ReservationRules,
            }, ct);

        if (r.EventDetails != null)
            await _listingRepository.UpsertDetailsAsync(listingId, new EventListingDetails
            {
                ListingId        = listingId,
                EventName        = r.EventDetails.EventName,
                Organizer        = r.EventDetails.Organizer,
                DateAndTime      = r.EventDetails.DateAndTime,
                SeatCount        = r.EventDetails.SeatCount,
                TicketPrice      = r.EventDetails.TicketPrice,
                EventType        = r.EventDetails.EventType,
                VenueName        = r.EventDetails.VenueName,
                VenueAddress     = r.EventDetails.VenueAddress,
                TicketTypesJson  = r.EventDetails.TicketTypes != null
                                   ? JsonSerializer.Serialize(r.EventDetails.TicketTypes)
                                   : null,
            }, ct);

        if (r.CarRentalDetails != null)
            await _listingRepository.UpsertDetailsAsync(listingId, new CarRentalListingDetails
            {
                ListingId          = listingId,
                Brand              = r.CarRentalDetails.Brand,
                Model              = r.CarRentalDetails.Model,
                Transmission       = r.CarRentalDetails.Transmission,
                PricePerDay        = r.CarRentalDetails.PricePerDay,
                SeatCount          = r.CarRentalDetails.SeatCount,
                FuelType           = r.CarRentalDetails.FuelType,
                AvailabilityStatus = r.CarRentalDetails.AvailabilityStatus,
                Year               = r.CarRentalDetails.Year,
                HourlyRate         = r.CarRentalDetails.HourlyRate,
                PickupLocation     = r.CarRentalDetails.PickupLocation,
                ReturnLocation     = r.CarRentalDetails.ReturnLocation,
                InsuranceOptions   = r.CarRentalDetails.InsuranceOptions,
            }, ct);

        if (r.ActivityDetails != null)
            await _listingRepository.UpsertDetailsAsync(listingId, new ActivityListingDetails
            {
                ListingId            = listingId,
                ActivityType         = r.ActivityDetails.ActivityType,
                DurationHours        = r.ActivityDetails.DurationHours,
                DifficultyLevel      = r.ActivityDetails.DifficultyLevel,
                Price                = r.ActivityDetails.Price,
                MinGroupSize         = r.ActivityDetails.MinGroupSize,
                MaxGroupSize         = r.ActivityDetails.MaxGroupSize,
                MinAge               = r.ActivityDetails.MinAge,
                MaxAge               = r.ActivityDetails.MaxAge,
                IncludedServices     = r.ActivityDetails.IncludedServices != null
                                       ? string.Join(",", r.ActivityDetails.IncludedServices)
                                       : null,
                SafetyRequirements   = r.ActivityDetails.SafetyRequirements,
                AvailabilitySchedule = r.ActivityDetails.AvailabilitySchedule,
            }, ct);
    }

    // ── Detail upserts (Update) ───────────────────────────────────────────────

    private async Task UpsertDetailsFromUpdateAsync(
        Guid listingId, UpdateListingRequest r, CancellationToken ct)
    {
        if (r.HotelDetails != null)
            await _listingRepository.UpsertDetailsAsync(listingId, new HotelListingDetails
            {
                ListingId       = listingId,
                PricePerNight   = r.HotelDetails.PricePerNight,
                AvailableRooms  = r.HotelDetails.AvailableRooms,
                Amenities       = string.Join(",", r.HotelDetails.Amenities),
                RoomTypes       = string.Join(",", r.HotelDetails.RoomTypes),
                CheckInTime     = r.HotelDetails.CheckInTime,
                CheckOutTime    = r.HotelDetails.CheckOutTime,
                PropertyType    = r.HotelDetails.PropertyType,
                PrimaryRoomType = r.HotelDetails.PrimaryRoomType,
            }, ct);

        if (r.RestaurantDetails != null)
            await _listingRepository.UpsertDetailsAsync(listingId, new RestaurantListingDetails
            {
                ListingId        = listingId,
                CuisineType      = r.RestaurantDetails.CuisineType,
                AverageCost      = r.RestaurantDetails.AverageCost,
                OpeningHours     = r.RestaurantDetails.OpeningHours,
                TableCapacity    = r.RestaurantDetails.TableCapacity,
                TableTypes       = r.RestaurantDetails.TableTypes != null
                                   ? string.Join(",", r.RestaurantDetails.TableTypes)
                                   : null,
                ReservationRules = r.RestaurantDetails.ReservationRules,
            }, ct);

        if (r.EventDetails != null)
            await _listingRepository.UpsertDetailsAsync(listingId, new EventListingDetails
            {
                ListingId        = listingId,
                EventName        = r.EventDetails.EventName,
                Organizer        = r.EventDetails.Organizer,
                DateAndTime      = r.EventDetails.DateAndTime,
                SeatCount        = r.EventDetails.SeatCount,
                TicketPrice      = r.EventDetails.TicketPrice,
                EventType        = r.EventDetails.EventType,
                VenueName        = r.EventDetails.VenueName,
                VenueAddress     = r.EventDetails.VenueAddress,
                TicketTypesJson  = r.EventDetails.TicketTypes != null
                                   ? JsonSerializer.Serialize(r.EventDetails.TicketTypes)
                                   : null,
            }, ct);

        if (r.CarRentalDetails != null)
            await _listingRepository.UpsertDetailsAsync(listingId, new CarRentalListingDetails
            {
                ListingId          = listingId,
                Brand              = r.CarRentalDetails.Brand,
                Model              = r.CarRentalDetails.Model,
                Transmission       = r.CarRentalDetails.Transmission,
                PricePerDay        = r.CarRentalDetails.PricePerDay,
                SeatCount          = r.CarRentalDetails.SeatCount,
                FuelType           = r.CarRentalDetails.FuelType,
                AvailabilityStatus = r.CarRentalDetails.AvailabilityStatus,
                Year               = r.CarRentalDetails.Year,
                HourlyRate         = r.CarRentalDetails.HourlyRate,
                PickupLocation     = r.CarRentalDetails.PickupLocation,
                ReturnLocation     = r.CarRentalDetails.ReturnLocation,
                InsuranceOptions   = r.CarRentalDetails.InsuranceOptions,
            }, ct);

        if (r.ActivityDetails != null)
            await _listingRepository.UpsertDetailsAsync(listingId, new ActivityListingDetails
            {
                ListingId            = listingId,
                ActivityType         = r.ActivityDetails.ActivityType,
                DurationHours        = r.ActivityDetails.DurationHours,
                DifficultyLevel      = r.ActivityDetails.DifficultyLevel,
                Price                = r.ActivityDetails.Price,
                MinGroupSize         = r.ActivityDetails.MinGroupSize,
                MaxGroupSize         = r.ActivityDetails.MaxGroupSize,
                MinAge               = r.ActivityDetails.MinAge,
                MaxAge               = r.ActivityDetails.MaxAge,
                IncludedServices     = r.ActivityDetails.IncludedServices != null
                                       ? string.Join(",", r.ActivityDetails.IncludedServices)
                                       : null,
                SafetyRequirements   = r.ActivityDetails.SafetyRequirements,
                AvailabilitySchedule = r.ActivityDetails.AvailabilitySchedule,
            }, ct);
    }
}