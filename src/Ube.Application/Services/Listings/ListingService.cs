using Ube.Application.DTOs.Listings;
using Ube.Application.Interfaces.Repositories;
using Ube.Domain.Entities.Listings;
using System.Text.Json;

namespace Ube.Application.Services.Listings;

public sealed class ListingService : IListingService
{
    private readonly IListingRepository _repository;

    public ListingService(IListingRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ListingResponseDto>> GetActiveListingsAsync(CancellationToken cancellationToken = default)
    {
        var listings = await _repository.GetActiveListingsAsync(cancellationToken);
        return listings.Select(MapToResponse).ToList();
    }

    public async Task<ListingResponseDto?> GetActiveListingByIdAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        var listing = await _repository.GetActiveListingByIdAsync(listingId, cancellationToken);
        return listing is null ? null : MapToResponse(listing);
    }

    public async Task<IReadOnlyList<ListingResponseDto>> GetVendorListingsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var listings = await _repository.GetVendorListingsAsync(userId, cancellationToken);
        return listings.Select(MapToResponse).ToList();
    }

    public async Task<ListingResponseDto> CreateAsync(Guid userId, CreateListingRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        if (!await _repository.CategoryExistsAsync(request.CategoryId, cancellationToken))
            throw new KeyNotFoundException("Category was not found or is inactive.");

        var vendorProfile = await _repository.GetVendorProfileAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("The authenticated user does not have a vendor profile.");

        var listing = new Listing
        {
            Id = Guid.NewGuid(), VendorProfileId = vendorProfile.Id, CategoryId = request.CategoryId,
            Title = request.Title.Trim(), Description = request.Description.Trim(), Price = request.Price,
            Currency = request.Currency.Trim().ToUpperInvariant(), Location = request.Location?.Trim(), IsActive = request.IsActive,
            ListingType = ResolveType(request),
            ImagesJson = JsonSerializer.Serialize(request.Images ?? new List<string>()),
            TagsJson = JsonSerializer.Serialize(request.Tags ?? new List<string>()),
            CancellationPolicy = request.CancellationPolicy?.Trim(),
            DetailsJson = SerializeDetails(request),
        };
        await _repository.AddAsync(listing, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        var created = await _repository.GetActiveListingByIdAsync(listing.Id, cancellationToken);
        return MapToResponse(created ?? listing);
    }

    public async Task<ListingResponseDto?> UpdateAsync(Guid userId, Guid listingId, CreateListingRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        if (!await _repository.CategoryExistsAsync(request.CategoryId, cancellationToken))
            throw new KeyNotFoundException("Category was not found or is inactive.");

        var listing = await _repository.GetOwnedListingAsync(listingId, userId, cancellationToken);
        if (listing is null) return null;

        listing.CategoryId = request.CategoryId;
        listing.Title = request.Title.Trim();
        listing.Description = request.Description.Trim();
        listing.Price = request.Price;
        listing.Currency = request.Currency.Trim().ToUpperInvariant();
        listing.Location = request.Location?.Trim();
        listing.IsActive = request.IsActive;
        listing.ListingType = ResolveType(request);
        listing.ImagesJson = JsonSerializer.Serialize(request.Images ?? new List<string>());
        listing.TagsJson = JsonSerializer.Serialize(request.Tags ?? new List<string>());
        listing.CancellationPolicy = request.CancellationPolicy?.Trim();
        listing.DetailsJson = SerializeDetails(request);
        listing.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(listing, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return MapToResponse(listing);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid listingId, CancellationToken cancellationToken = default)
    {
        var listing = await _repository.GetOwnedListingAsync(listingId, userId, cancellationToken);
        if (listing is null) return false;
        await _repository.DeleteAsync(listing, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void Validate(CreateListingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title)) throw new ArgumentException("Title is required.");
        if (string.IsNullOrWhiteSpace(request.Description)) throw new ArgumentException("Description is required.");
        if (request.Price <= 0) throw new ArgumentException("Price must be greater than zero.");
        if (string.IsNullOrWhiteSpace(request.Currency) || request.Currency.Trim().Length != 3)
            throw new ArgumentException("Currency must be a three-letter code.");
    }

    private static string ResolveType(CreateListingRequest request)
    {
        if (request.HotelDetails is not null) return "Hotel";
        if (request.RestaurantDetails is not null) return "Restaurant";
        if (request.EventDetails is not null) return "Event";
        if (request.CarRentalDetails is not null) return "CarRental";
        return "Activity";
    }

    private static string? SerializeDetails(CreateListingRequest request)
    {
        object? details = request.HotelDetails is not null ? request.HotelDetails
            : request.RestaurantDetails is not null ? request.RestaurantDetails
            : request.EventDetails is not null ? request.EventDetails
            : request.CarRentalDetails is not null ? request.CarRentalDetails
            : request.ActivityDetails;
        return details is null ? null : JsonSerializer.Serialize(details);
    }

    private static T? DeserializeDetails<T>(Listing listing) where T : class
    {
        if (string.IsNullOrWhiteSpace(listing.DetailsJson)) return null;
        try { return JsonSerializer.Deserialize<T>(listing.DetailsJson); }
        catch (JsonException) { return null; }
    }

    private static List<string> DeserializeList(string json)
    {
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>(); }
        catch (JsonException) { return new List<string>(); }
    }

    private static ListingResponseDto MapToResponse(Listing listing)
    {
        return new ListingResponseDto
        {
            Id = listing.Id,
            VendorProfileId = listing.VendorProfileId,
            CategoryId = listing.CategoryId,
            Title = listing.Title,
            Description = listing.Description,
            Price = listing.Price,
            Currency = listing.Currency,
            Location = listing.Location,
            IsActive = listing.IsActive,
            CategoryName = listing.Category?.Name ?? string.Empty,
            VendorName = listing.VendorProfile?.BusinessName ?? string.Empty,
            AverageRating = listing.Reviews.Count == 0 ? 0 : listing.Reviews.Average(review => review.Rating),
            TotalReviews = listing.Reviews.Count,
            Type = listing.ListingType,
            PrimaryImage = DeserializeList(listing.ImagesJson).FirstOrDefault(),
            Images = DeserializeList(listing.ImagesJson),
            Tags = DeserializeList(listing.TagsJson),
            CancellationPolicy = listing.CancellationPolicy,
            HotelDetails = listing.ListingType == "Hotel" ? DeserializeDetails<HotelDetailsDto>(listing) : null,
            RestaurantDetails = listing.ListingType == "Restaurant" ? DeserializeDetails<RestaurantDetailsDto>(listing) : null,
            CarRentalDetails = listing.ListingType == "CarRental" ? DeserializeDetails<CarRentalDetailsDto>(listing) : null,
            ActivityDetails = listing.ListingType == "Activity" ? DeserializeDetails<ActivityDetailsDto>(listing) : null,
            EventDetails = listing.ListingType == "Event" ? DeserializeDetails<EventDetailsDto>(listing) : null,
        };
    }
}