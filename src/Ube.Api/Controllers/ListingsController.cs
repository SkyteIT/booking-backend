using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public async Task<IActionResult> CreateListing([FromBody] CreateListingRequest request)
    {
        if (!await _context.VendorProfiles.AnyAsync(v => v.Id == request.VendorId))
            return BadRequest("Invalid VendorId.");

        if (!await _context.Categories.AnyAsync(c => c.Id == request.CategoryId))
            return BadRequest("Invalid CategoryId.");

        var listing = new Listing
        {
            Id = Guid.NewGuid(),
            VendorProfileId = request.VendorId,
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
                CheckOutTime = h.CheckOutTime
            });
        }

        // ================= RESTAURANT =================
        if (request.Type == ListingType.Restaurant && request.RestaurantDetails != null)
        {
            var r = request.RestaurantDetails;

            _context.Set<RestaurantListingDetails>().Add(new RestaurantListingDetails
            {
                Id = Guid.NewGuid(),
                ListingId = listing.Id,
                CuisineType = r.CuisineType,
                AverageCost = r.AverageCost,
                OpeningHours = r.OpeningHours,
                TableCapacity = r.TableCapacity
            });
        }

        // ================= EVENT =================
        if (request.Type == ListingType.Event && request.EventDetails != null)
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
                TicketPrice = e.TicketPrice
            });
        }

        // ================= CAR RENTAL =================
        if (request.Type == ListingType.CarRental && request.CarRentalDetails != null)
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
                AvailabilityStatus = c.AvailabilityStatus
            });
        }

        // ================= ACTIVITY =================
        if (request.Type == ListingType.Activity && request.ActivityDetails != null)
        {
            var a = request.ActivityDetails;

            _context.Set<ActivityListingDetails>().Add(new ActivityListingDetails
            {
                Id = Guid.NewGuid(),
                ListingId = listing.Id,
                ActivityType = a.ActivityType,
                DurationHours = a.DurationHours,
                DifficultyLevel = a.DifficultyLevel,
                Price = a.Price
            });
        }

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetListingById), new { id = listing.Id }, listing.Id);
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
    public async Task<IActionResult> UpdateListing(Guid id, [FromBody] UpdateListingRequest request)
    {
        var listing = await _context.Listings.FindAsync(id);

        if (listing == null)
            return NotFound("Listing not found.");

        listing.VendorProfileId = request.VendorId;
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
    public async Task<IActionResult> DeleteListing(Guid id)
    {
        var listing = await _context.Listings.FindAsync(id);

        if (listing == null)
            return NotFound("Listing not found.");

        _context.Listings.Remove(listing);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}