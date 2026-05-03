using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Ube.Application.Features.Listings.Commands;
using Ube.Application.Features.Listings.Queries;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Enums.Listings;
using Ube.Infrastructure.Persistence;

namespace Ube.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ListingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ListingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ================= CREATE =================
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateListing([FromBody] CreateListingRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("User ID not found in token.");
        }

        var vendor = await _context.VendorProfiles
            .Where(v => v.UserId == userId)
            .FirstOrDefaultAsync();

        if (vendor == null)
        {
            return Forbid("You do not have a vendor profile to create listings.");
        }

        // We override request.VendorId with the authenticated user's vendor ID
        var vendorId = vendor.Id;

        if (!await _context.Categories.AnyAsync(c => c.Id == request.CategoryId))
            return BadRequest("Invalid CategoryId.");

        var listing = new Listing
        {
            Id = Guid.NewGuid(),
            VendorProfileId = vendorId,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            BasePrice = request.BasePrice,
            Currency = request.Currency,
            Location = request.Location,
            Tags = request.Tags != null ? string.Join(", ", request.Tags) : null,
            CancellationPolicy = request.CancellationPolicy,
            IsAvailable = request.IsAvailable,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Type = request.Type
        };

        _context.Listings.Add(listing);

        // ================= IMAGES =================
        if (request.Images != null && request.Images.Any())
        {
            foreach (var imageUrl in request.Images)
            {
                _context.ListingImages.Add(new ListingImage
                {
                    Id = Guid.NewGuid(),
                    ListingId = listing.Id,
                    ImageUrl = imageUrl,
                    IsPrimary = imageUrl == request.Images.First()
                });
            }
        }

        await _context.SaveChangesAsync();

        // ================= HOTEL =================
        if (request.Type == ListingType.Hotel && request.HotelDetails != null)
        {
            var h = request.HotelDetails;
            _context.Set<HotelListingDetails>().Add(new HotelListingDetails
            {
                Id = Guid.NewGuid(),
                ListingId = listing.Id,
                PricePerNight = h.PricePerNight,
                AvailableRooms = h.AvailableRooms,
                Amenities = string.Join(", ", h.Amenities),
                RoomTypes = string.Join(", ", h.RoomTypes),
                CheckInTime = h.CheckInTime,
                CheckOutTime = h.CheckOutTime,
                PropertyType = h.PropertyType,
                PrimaryRoomType = h.PrimaryRoomType,
                Images = request.Images != null ? string.Join(", ", request.Images) : null
            });
        }

        // ================= RESTAURANT =================
        else if (request.Type == ListingType.Restaurant && request.RestaurantDetails != null)
        {
            var r = request.RestaurantDetails;
            _context.Set<RestaurantListingDetails>().Add(new RestaurantListingDetails
            {
                Id = Guid.NewGuid(),
                ListingId = listing.Id,
                CuisineType = r.CuisineType,
                AverageCost = r.AverageCost,
                OpeningHours = r.OpeningHours,
                TableCapacity = r.TableCapacity,
                TableTypes = r.TableTypes != null ? string.Join(", ", r.TableTypes) : null,
                ReservationRules = r.ReservationRules,
                Images = request.Images != null ? string.Join(", ", request.Images) : null
            });
        }

        // ================= EVENT =================
        else if (request.Type == ListingType.Event && request.EventDetails != null)
        {
            var e = request.EventDetails;
            _context.Set<EventListingDetails>().Add(new EventListingDetails
            {
                Id = Guid.NewGuid(),
                ListingId = listing.Id,
                EventName = e.EventName,
                Organizer = e.Organizer,
                DateAndTime = e.DateAndTime,
                SeatCount = e.SeatCount,
                TicketPrice = e.TicketPrice,
                EventType = e.EventType,
                VenueName = e.VenueName,
                VenueAddress = e.VenueAddress,
                TicketTypesJson = e.TicketTypes != null ? System.Text.Json.JsonSerializer.Serialize(e.TicketTypes) : null,
                Images = request.Images != null ? string.Join(", ", request.Images) : null
            });
        }

        // ================= CAR RENTAL =================
        else if (request.Type == ListingType.CarRental && request.CarRentalDetails != null)
        {
            var c = request.CarRentalDetails;
            _context.Set<CarRentalListingDetails>().Add(new CarRentalListingDetails
            {
                Id = Guid.NewGuid(),
                ListingId = listing.Id,
                Brand = c.Brand,
                Model = c.Model,
                Transmission = c.Transmission,
                PricePerDay = c.PricePerDay,
                SeatCount = c.SeatCount,
                FuelType = c.FuelType,
                AvailabilityStatus = c.AvailabilityStatus,
                Year = c.Year,
                HourlyRate = c.HourlyRate,
                PickupLocation = c.PickupLocation,
                ReturnLocation = c.ReturnLocation,
                InsuranceOptions = c.InsuranceOptions,
                Images = request.Images != null ? string.Join(", ", request.Images) : null
            });
        }

        // ================= ACTIVITY =================
        else if (request.Type == ListingType.Activity && request.ActivityDetails != null)
        {
            var a = request.ActivityDetails;
            _context.Set<ActivityListingDetails>().Add(new ActivityListingDetails
            {
                Id = Guid.NewGuid(),
                ListingId = listing.Id,
                ActivityType = a.ActivityType,
                DurationHours = a.DurationHours,
                DifficultyLevel = a.DifficultyLevel,
                Price = a.Price,
                MinGroupSize = a.MinGroupSize,
                MaxGroupSize = a.MaxGroupSize,
                MinAge = a.MinAge,
                MaxAge = a.MaxAge,
                IncludedServices = a.IncludedServices != null ? string.Join(", ", a.IncludedServices) : null,
                SafetyRequirements = a.SafetyRequirements,
                AvailabilitySchedule = a.AvailabilitySchedule,
                Images = request.Images != null ? string.Join(", ", request.Images) : null
            });
        }

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetListingById), new { id = listing.Id }, listing.Id);
    }

    // ================= GET MY LISTINGS =================
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyListings()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("User ID not found in token.");
        }

        var vendor = await _context.VendorProfiles
            .FirstOrDefaultAsync(v => v.UserId == userId);

        if (vendor == null)
            return Forbid("No vendor profile found.");

        var listings = await _context.Listings
            .Where(l => l.VendorProfileId == vendor.Id)
            .Include(l => l.Category)
            .Include(l => l.Images)
            .Select(l => new ListingResponse
            {
                Id = l.Id,
                VendorId = l.VendorProfileId,
                CategoryId = l.CategoryId,
                Title = l.Title,
                Description = l.Description,
                BasePrice = l.BasePrice,
                Currency = l.Currency,
                Location = l.Location,
                IsActive = l.IsActive,
                CategoryName = l.Category != null ? l.Category.Name : "Other",
                VendorName = vendor.BusinessName,
                Type = l.Type,
                Status = l.IsActive ? "Live" : "Inactive",
                PrimaryImage = l.Images.OrderByDescending(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault(),
                BookingsCount = _context.Bookings.Count(b => b.ListingId == l.Id),
                Rating = _context.Reviews.Where(r => r.ListingId == l.Id).Select(r => (double?)r.Rating).Average() ?? 0
            })
            .ToListAsync();

        return Ok(listings);
    }

    // ================= GET =================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetListingById(Guid id)
    {
        var listing = await _context.Listings
            .Include(l => l.Category)
            .Include(l => l.Vendor)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (listing == null)
            return NotFound();

        var dto = new ListingResponse
        {
            Id = listing.Id,
            VendorId = listing.VendorProfileId,
            CategoryId = listing.CategoryId,
            Title = listing.Title,
            Description = listing.Description,
            BasePrice = listing.BasePrice,
            Currency = listing.Currency,
            Location = listing.Location,
            IsActive = listing.IsActive,
            CategoryName = listing.Category?.Name ?? "",
            VendorName = listing.Vendor?.BusinessName ?? "",
            Type = listing.Type
        };

        return Ok(dto);
    }

    // ================= UPDATE =================
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateListing(Guid id, [FromBody] UpdateListingRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("User ID not found in token.");
        }

        var listing = await _context.Listings
            .Include(l => l.Vendor)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (listing == null)
            return NotFound("Listing not found.");

        if (listing.Vendor == null || listing.Vendor.UserId != userId)
        {
            return Forbid("You do not have permission to update this listing.");
        }

        // We ignore request.VendorId and keep the existing ownership
        listing.CategoryId = request.CategoryId;
        listing.Title = request.Title;
        listing.Description = request.Description;
        listing.BasePrice = request.BasePrice;
        listing.Currency = request.Currency;
        listing.Location = request.Location;
        listing.IsActive = request.IsActive;
        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok();
    }

    // ================= DELETE =================
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteListing(Guid id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("User ID not found in token.");
        }

        var listing = await _context.Listings
            .Include(l => l.Vendor)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (listing == null)
            return NotFound("Listing not found.");

        if (listing.Vendor == null || listing.Vendor.UserId != userId)
        {
            return Forbid("You do not have permission to delete this listing.");
        }

        _context.Listings.Remove(listing);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}