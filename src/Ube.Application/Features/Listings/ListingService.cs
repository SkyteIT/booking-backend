using Ube.Application.Common.Interfaces.Services;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Exceptions;
using Ube.Application.Features.Content.Category;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Enums.Listings;
namespace Ube.Application.Features.Listings;

public class ListingService : IListingService
{
    private readonly IListingRepository _listingRepository;
    private readonly IVendorProfileRepository _vendorProfileRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ListingService(
        IListingRepository listingRepository,
        IVendorProfileRepository vendorProfileRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _listingRepository = listingRepository;
        _vendorProfileRepository = vendorProfileRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateListingAsync(Guid userId, CreateListingRequest request, CancellationToken ct = default)
    {
        var vendorProfile = await _vendorProfileRepository.GetVendorIdAsync(userId)
            ?? throw new NotFoundException("Vendor profile not found for the current user.");

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, ct: ct)
            ?? throw new NotFoundException("Category not found.");

        var type = category.Type
            ?? throw new BusinessRuleException(
                $"Category '{category.Name}' has no listing type configured yet; an admin must set one before listings can be created under it.");

        EnsureDetailProvided(type, request.HotelDetails, request.RestaurantDetails, request.EventDetails,
            request.CarRentalDetails, request.ActivityDetails);

        EnsureRequiredCustomFieldsProvided(category, request.CustomFieldValues);

        var listing = new Listing
        {
            Id = Guid.NewGuid(),
            VendorProfileId = vendorProfile.Id,
            CategoryId = request.CategoryId,
            Type = type,
            Title = request.Title,
            Description = request.Description ?? string.Empty,
            Price = request.Price,
            Currency = request.Currency,
            Location = request.Location,
            IsActive = request.IsActive,
            Tags = request.Tags.Count > 0 ? string.Join(", ", request.Tags) : null,
            CancellationPolicy = request.CancellationPolicy,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _listingRepository.AddAsync(listing, ct);

            if (request.Images.Count > 0)
                await _listingRepository.ReplaceImagesAsync(listing.Id, request.Images, ct);

            await UpsertTypeDetailsAsync(listing.Id, type,
                request.HotelDetails, request.RestaurantDetails, request.EventDetails,
                request.CarRentalDetails, request.ActivityDetails, ct);

            await _listingRepository.ReplaceCustomFieldValuesAsync(listing.Id, MapCustomFieldValues(request.CustomFieldValues), ct);

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        return listing.Id;
    }

    public async Task UpdateListingAsync(Guid listingId, Guid userId, UpdateListingRequest request, CancellationToken ct = default)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId)
            ?? throw new NotFoundException("Listing not found.");

        var vendorProfile = await _vendorProfileRepository.GetVendorIdAsync(userId);
        if (vendorProfile == null || vendorProfile.Id != listing.VendorProfileId)
            throw new ForbiddenException("You do not have permission to update this listing.");

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, ct: ct)
            ?? throw new NotFoundException("Category not found.");

        var type = category.Type
            ?? throw new BusinessRuleException(
                $"Category '{category.Name}' has no listing type configured yet; an admin must set one before listings can use it.");

        EnsureDetailProvided(type, request.HotelDetails, request.RestaurantDetails, request.EventDetails,
            request.CarRentalDetails, request.ActivityDetails);

        EnsureRequiredCustomFieldsProvided(category, request.CustomFieldValues);

        listing.CategoryId = request.CategoryId;
        listing.Type = type;
        listing.Title = request.Title;
        listing.Description = request.Description ?? string.Empty;
        listing.Price = request.Price;
        listing.Currency = request.Currency;
        listing.Location = request.Location;
        listing.IsActive = request.IsActive;
        listing.Tags = request.Tags.Count > 0 ? string.Join(", ", request.Tags) : null;
        listing.CancellationPolicy = request.CancellationPolicy;
        listing.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _listingRepository.UpdateAsync(listing);
            await _listingRepository.ReplaceImagesAsync(listingId, request.Images, ct);
            await UpsertTypeDetailsAsync(listingId, type,
                request.HotelDetails, request.RestaurantDetails, request.EventDetails,
                request.CarRentalDetails, request.ActivityDetails, ct);

            await _listingRepository.ReplaceCustomFieldValuesAsync(listingId, MapCustomFieldValues(request.CustomFieldValues), ct);

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteListingAsync(Guid listingId, Guid userId, CancellationToken ct = default)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId)
            ?? throw new NotFoundException("Listing not found.");

        var vendorProfile = await _vendorProfileRepository.GetVendorIdAsync(userId);
        if (vendorProfile == null || vendorProfile.Id != listing.VendorProfileId)
            throw new ForbiddenException("You do not have permission to delete this listing.");

        await _listingRepository.DeleteAsync(listing, ct);
    }

    public async Task<ListingResponse?> GetListingByIdAsync(Guid listingId, CancellationToken ct = default)
    {
        var listing = await _listingRepository.GetByIdWithDetailsAsync(listingId, ct);
        return listing == null ? null : MapToResponse(listing);
    }

    public async Task<List<ListingResponse>> GetAllListingsAsync(CancellationToken ct = default)
    {
        var listings = await _listingRepository.GetAllWithDetailsAsync(ct);
        return listings.Select(MapToResponse).ToList();
    }

    public async Task<List<ListingResponse>> GetMyListingsAsync(Guid userId, CancellationToken ct = default)
    {
        var vendorProfile = await _vendorProfileRepository.GetVendorIdAsync(userId)
            ?? throw new NotFoundException("Vendor profile not found for the current user.");

        var listings = await _listingRepository.GetByVendorProfileIdWithDetailsAsync(vendorProfile.Id, ct);
        return listings.Select(MapToResponse).ToList();
    }

    private static void EnsureDetailProvided(
        ListingType type,
        HotelDetailsDto? hotel,
        RestaurantDetailsDto? restaurant,
        EventDetailsDto? @event,
        CarRentalDetailsDto? carRental,
        ActivityDetailsDto? activity)
    {
        var missing = type switch
        {
            ListingType.Hotel => hotel == null,
            ListingType.Restaurant => restaurant == null,
            ListingType.Event => @event == null,
            ListingType.CarRental => carRental == null,
            ListingType.Activity => activity == null,
            _ => true
        };

        if (missing)
            throw new BusinessRuleException($"{type} details are required for a {type} listing.");
    }

    private static void EnsureRequiredCustomFieldsProvided(
        Ube.Domain.Entities.Listings.Category category,
        List<ListingCustomFieldValueInputDto> values)
    {
        var provided = values
            .Where(v => !string.IsNullOrWhiteSpace(v.Value))
            .Select(v => v.CategoryCustomFieldId)
            .ToHashSet();

        var missing = category.CustomFields
            .Where(f => f.Required && !provided.Contains(f.Id))
            .Select(f => f.Label)
            .ToList();

        if (missing.Count > 0)
            throw new BusinessRuleException($"Missing required field(s): {string.Join(", ", missing)}.");
    }

    private static List<ListingCustomFieldValue> MapCustomFieldValues(IEnumerable<ListingCustomFieldValueInputDto> values)
        => values.Select(v => new ListingCustomFieldValue
        {
            Id = Guid.NewGuid(),
            CategoryCustomFieldId = v.CategoryCustomFieldId,
            Value = v.Value
        }).ToList();

    private Task UpsertTypeDetailsAsync(
        Guid listingId,
        ListingType type,
        HotelDetailsDto? hotel,
        RestaurantDetailsDto? restaurant,
        EventDetailsDto? @event,
        CarRentalDetailsDto? carRental,
        ActivityDetailsDto? activity,
        CancellationToken ct)
    {
        return type switch
        {
            ListingType.Hotel when hotel != null => _listingRepository.UpsertDetailsAsync(listingId, new HotelListingDetails
            {
                Id = Guid.NewGuid(),
                PricePerNight = hotel.PricePerNight,
                AvailableRooms = hotel.AvailableRooms,
                Amenities = string.Join(", ", hotel.Amenities),
                RoomTypes = string.Join(", ", hotel.RoomTypes),
                CheckInTime = hotel.CheckInTime,
                CheckOutTime = hotel.CheckOutTime,
                PropertyType = hotel.PropertyType,
                PrimaryRoomType = hotel.PrimaryRoomType
            }, ct),

            ListingType.Restaurant when restaurant != null => _listingRepository.UpsertDetailsAsync(listingId, new RestaurantListingDetails
            {
                Id = Guid.NewGuid(),
                CuisineType = restaurant.CuisineType,
                AverageCost = restaurant.AverageCost,
                OpeningHours = restaurant.OpeningHours,
                TableCapacity = restaurant.TableCapacity,
                TableTypes = restaurant.TableTypes != null ? string.Join(", ", restaurant.TableTypes) : null,
                ReservationRules = restaurant.ReservationRules
            }, ct),

            ListingType.Event when @event != null => _listingRepository.UpsertDetailsAsync(listingId, new EventListingDetails
            {
                Id = Guid.NewGuid(),
                EventName = @event.EventName,
                Organizer = @event.Organizer,
                DateAndTime = @event.DateAndTime,
                SeatCount = @event.SeatCount,
                TicketPrice = @event.TicketPrice,
                EventType = @event.EventType,
                VenueName = @event.VenueName,
                VenueAddress = @event.VenueAddress,
                TicketTypesJson = @event.TicketTypes != null ? System.Text.Json.JsonSerializer.Serialize(@event.TicketTypes) : null
            }, ct),

            ListingType.CarRental when carRental != null => _listingRepository.UpsertDetailsAsync(listingId, new CarRentalListingDetails
            {
                Id = Guid.NewGuid(),
                Brand = carRental.Brand,
                Model = carRental.Model,
                Transmission = carRental.Transmission,
                PricePerDay = carRental.PricePerDay,
                SeatCount = carRental.SeatCount,
                FuelType = carRental.FuelType,
                AvailabilityStatus = carRental.AvailabilityStatus,
                Year = carRental.Year,
                HourlyRate = carRental.HourlyRate,
                PickupLocation = carRental.PickupLocation,
                ReturnLocation = carRental.ReturnLocation,
                InsuranceOptions = carRental.InsuranceOptions
            }, ct),

            ListingType.Activity when activity != null => _listingRepository.UpsertDetailsAsync(listingId, new ActivityListingDetails
            {
                Id = Guid.NewGuid(),
                ActivityType = activity.ActivityType,
                DurationHours = activity.DurationHours,
                DifficultyLevel = activity.DifficultyLevel,
                Price = activity.Price,
                MinGroupSize = activity.MinGroupSize,
                MaxGroupSize = activity.MaxGroupSize,
                MinAge = activity.MinAge,
                MaxAge = activity.MaxAge,
                IncludedServices = activity.IncludedServices != null ? string.Join(", ", activity.IncludedServices) : null,
                SafetyRequirements = activity.SafetyRequirements,
                AvailabilitySchedule = activity.AvailabilitySchedule
            }, ct),

            _ => Task.CompletedTask
        };
    }

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
        PricingUnit = l.Category?.ServiceModel,
        Type = l.Type,
        AverageRating = l.AverageRating,
        TotalReviews = l.TotalReviews,
        PrimaryImage = l.Images?.OrderByDescending(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault(),
        Images = l.Images?.Select(i => i.ImageUrl).ToList() ?? new List<string>(),
        Tags = l.Tags != null ? l.Tags.Split(", ", StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>(),
        CancellationPolicy = l.CancellationPolicy,
        HasActiveOffer = ActiveOffer(l) != null,
        OfferBadgeText = FormatOfferBadge(ActiveOffer(l)),
        HotelDetails = l.HotelDetails == null ? null : new HotelDetailsDto
        {
            PricePerNight = l.HotelDetails.PricePerNight,
            AvailableRooms = l.HotelDetails.AvailableRooms,
            Amenities = l.HotelDetails.Amenities.Split(", ", StringSplitOptions.RemoveEmptyEntries).ToList(),
            RoomTypes = l.HotelDetails.RoomTypes.Split(", ", StringSplitOptions.RemoveEmptyEntries).ToList(),
            CheckInTime = l.HotelDetails.CheckInTime,
            CheckOutTime = l.HotelDetails.CheckOutTime,
            PropertyType = l.HotelDetails.PropertyType,
            PrimaryRoomType = l.HotelDetails.PrimaryRoomType
        },
        RestaurantDetails = l.RestaurantDetails == null ? null : new RestaurantDetailsDto
        {
            CuisineType = l.RestaurantDetails.CuisineType,
            AverageCost = l.RestaurantDetails.AverageCost,
            OpeningHours = l.RestaurantDetails.OpeningHours,
            TableCapacity = l.RestaurantDetails.TableCapacity,
            TableTypes = l.RestaurantDetails.TableTypes?.Split(", ", StringSplitOptions.RemoveEmptyEntries).ToList(),
            ReservationRules = l.RestaurantDetails.ReservationRules
        },
        EventDetails = l.EventDetails == null ? null : new EventDetailsDto
        {
            EventName = l.EventDetails.EventName,
            Organizer = l.EventDetails.Organizer,
            DateAndTime = l.EventDetails.DateAndTime,
            SeatCount = l.EventDetails.SeatCount,
            TicketPrice = l.EventDetails.TicketPrice,
            EventType = l.EventDetails.EventType,
            VenueName = l.EventDetails.VenueName,
            VenueAddress = l.EventDetails.VenueAddress,
            TicketTypes = l.EventDetails.TicketTypesJson != null
                ? System.Text.Json.JsonSerializer.Deserialize<List<TicketTypeDto>>(l.EventDetails.TicketTypesJson)
                : null
        },
        CarRentalDetails = l.CarRentalDetails == null ? null : new CarRentalDetailsDto
        {
            Brand = l.CarRentalDetails.Brand,
            Model = l.CarRentalDetails.Model,
            Transmission = l.CarRentalDetails.Transmission,
            PricePerDay = l.CarRentalDetails.PricePerDay,
            SeatCount = l.CarRentalDetails.SeatCount,
            FuelType = l.CarRentalDetails.FuelType,
            AvailabilityStatus = l.CarRentalDetails.AvailabilityStatus,
            Year = l.CarRentalDetails.Year,
            HourlyRate = l.CarRentalDetails.HourlyRate,
            PickupLocation = l.CarRentalDetails.PickupLocation,
            ReturnLocation = l.CarRentalDetails.ReturnLocation,
            InsuranceOptions = l.CarRentalDetails.InsuranceOptions
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

    // Any currently-active offer (perk-only included) - "does this
    // listing have something running right now," same rule the search
    // endpoint uses. Requires l.Offers to have been Include()'d.
    private static ListingOffer? ActiveOffer(Listing l)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return l.Offers?.FirstOrDefault(o => o.IsActive && o.StartDate <= today && o.EndDate >= today);
    }

    private static string? FormatOfferBadge(ListingOffer? offer) => offer?.DiscountType switch
    {
        null when offer == null => null,
        OfferDiscountType.PercentageDiscount => $"{offer.DiscountValue:0.##}% OFF",
        OfferDiscountType.FixedAmountDiscount => $"{offer.DiscountValue:0.##} OFF",
        _ => "Special Offer"
    };
}