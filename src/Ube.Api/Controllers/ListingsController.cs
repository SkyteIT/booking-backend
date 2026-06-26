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

    // ================= GET ALL =================
    [HttpGet]
    public async Task<IActionResult> GetAllListings()
    {
        var listings = await _context.Listings
            .Include(l => l.Category)
            .Include(l => l.Images)
            .Include(l => l.Vendor)
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
                VendorName = l.Vendor != null ? l.Vendor.BusinessName : "",
                Type = l.Type,
                Status = l.IsActive ? "Live" : "Inactive",
                PrimaryImage = l.Images.OrderByDescending(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault(),
                BookingsCount = _context.Bookings.Count(b => b.ListingId == l.Id),
                Rating = _context.Reviews.Where(r => r.ListingId == l.Id).Select(r => (double?)r.Rating).Average() ?? 0
            })
            .ToListAsync();

        return Ok(listings);
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
            .Include(l => l.Images)
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
            Status = listing.IsActive ? "Live" : "Inactive",
            CategoryName = listing.Category?.Name ?? "",
            VendorName = listing.Vendor?.BusinessName ?? "",
            Type = listing.Type,
            Images = listing.Images.Select(i => i.ImageUrl).ToList(),
            Tags = listing.Tags != null ? listing.Tags.Split(", ").ToList() : new List<string>(),
            CancellationPolicy = listing.CancellationPolicy
        };

        // Load type-specific details
        if (listing.Type == ListingType.Hotel)
        {
            var h = await _context.Set<HotelListingDetails>().FirstOrDefaultAsync(x => x.ListingId == id);
            if (h != null)
            {
                dto.HotelDetails = new HotelDetailsDto
                {
                    PricePerNight = h.PricePerNight,
                    AvailableRooms = h.AvailableRooms,
                    Amenities = h.Amenities?.Split(", ").ToList() ?? new List<string>(),
                    RoomTypes = h.RoomTypes?.Split(", ").ToList() ?? new List<string>(),
                    CheckInTime = h.CheckInTime,
                    CheckOutTime = h.CheckOutTime,
                    PropertyType = h.PropertyType,
                    PrimaryRoomType = h.PrimaryRoomType
                };
            }
        }
        else if (listing.Type == ListingType.Restaurant)
        {
            var r = await _context.Set<RestaurantListingDetails>().FirstOrDefaultAsync(x => x.ListingId == id);
            if (r != null)
            {
                dto.RestaurantDetails = new RestaurantDetailsDto
                {
                    CuisineType = r.CuisineType,
                    AverageCost = r.AverageCost,
                    OpeningHours = r.OpeningHours,
                    TableCapacity = r.TableCapacity,
                    TableTypes = r.TableTypes?.Split(", ").ToList(),
                    ReservationRules = r.ReservationRules
                };
            }
        }
        else if (listing.Type == ListingType.Event)
        {
            var e = await _context.Set<EventListingDetails>().FirstOrDefaultAsync(x => x.ListingId == id);
            if (e != null)
            {
                dto.EventDetails = new EventDetailsDto
                {
                    EventName = e.EventName,
                    Organizer = e.Organizer,
                    DateAndTime = e.DateAndTime,
                    SeatCount = e.SeatCount,
                    TicketPrice = e.TicketPrice,
                    EventType = e.EventType,
                    VenueName = e.VenueName,
                    VenueAddress = e.VenueAddress,
                    TicketTypes = e.TicketTypesJson != null ? System.Text.Json.JsonSerializer.Deserialize<List<TicketTypeDto>>(e.TicketTypesJson) : null
                };
            }
        }
        else if (listing.Type == ListingType.CarRental)
        {
            var cr = await _context.Set<CarRentalListingDetails>().FirstOrDefaultAsync(x => x.ListingId == id);
            if (cr != null)
            {
                dto.CarRentalDetails = new CarRentalDetailsDto
                {
                    Brand = cr.Brand,
                    Model = cr.Model,
                    Transmission = cr.Transmission,
                    PricePerDay = cr.PricePerDay,
                    SeatCount = cr.SeatCount,
                    FuelType = cr.FuelType,
                    AvailabilityStatus = cr.AvailabilityStatus,
                    Year = cr.Year,
                    HourlyRate = cr.HourlyRate,
                    PickupLocation = cr.PickupLocation,
                    ReturnLocation = cr.ReturnLocation,
                    InsuranceOptions = cr.InsuranceOptions
                };
            }
        }
        else if (listing.Type == ListingType.Activity)
        {
            var a = await _context.Set<ActivityListingDetails>().FirstOrDefaultAsync(x => x.ListingId == id);
            if (a != null)
            {
                dto.ActivityDetails = new ActivityDetailsDto
                {
                    ActivityType = a.ActivityType,
                    DurationHours = a.DurationHours,
                    DifficultyLevel = a.DifficultyLevel,
                    Price = a.Price ?? 0,
                    MinGroupSize = a.MinGroupSize,
                    MaxGroupSize = a.MaxGroupSize,
                    MinAge = a.MinAge,
                    MaxAge = a.MaxAge,
                    IncludedServices = a.IncludedServices?.Split(", ").ToList(),
                    SafetyRequirements = a.SafetyRequirements,
                    AvailabilitySchedule = a.AvailabilitySchedule
                };
            }
        }

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
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (listing == null)
            return NotFound("Listing not found.");

        if (listing.Vendor == null || listing.Vendor.UserId != userId)
        {
            return Forbid("You do not have permission to update this listing.");
        }

        // Update base fields
        listing.CategoryId = request.CategoryId;
        listing.Title = request.Title;
        listing.Description = request.Description;
        listing.BasePrice = request.BasePrice;
        listing.Currency = request.Currency;
        listing.Location = request.Location;
        listing.IsActive = !string.IsNullOrEmpty(request.Status) && 
                          (request.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) || 
                           request.Status.Equals("Live", StringComparison.OrdinalIgnoreCase));
        listing.IsAvailable = request.IsAvailable;
        listing.Type = request.Type;
        listing.Tags = request.Tags != null ? string.Join(", ", request.Tags) : null;
        listing.CancellationPolicy = request.CancellationPolicy;
        listing.UpdatedAt = DateTime.UtcNow;

        // Update Images - Surgical replacement to ensure persistence
        var existingImages = await _context.ListingImages.Where(i => i.ListingId == id).ToListAsync();
        if (existingImages.Any())
        {
            _context.ListingImages.RemoveRange(existingImages);
            await _context.SaveChangesAsync(); 
        }

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

        // Update specific details
        if (request.Type == ListingType.Hotel && request.HotelDetails != null)
        {
            var h = await _context.Set<HotelListingDetails>().FirstOrDefaultAsync(x => x.ListingId == id);
            if (h == null)
            {
                h = new HotelListingDetails { Id = Guid.NewGuid(), ListingId = id };
                _context.Set<HotelListingDetails>().Add(h);
            }
            h.PricePerNight = request.HotelDetails.PricePerNight;
            h.AvailableRooms = request.HotelDetails.AvailableRooms;
            h.Amenities = string.Join(", ", request.HotelDetails.Amenities);
            h.RoomTypes = string.Join(", ", request.HotelDetails.RoomTypes);
            h.CheckInTime = request.HotelDetails.CheckInTime;
            h.CheckOutTime = request.HotelDetails.CheckOutTime;
            h.PropertyType = request.HotelDetails.PropertyType;
            h.PrimaryRoomType = request.HotelDetails.PrimaryRoomType;
            h.Images = request.Images != null ? string.Join(", ", request.Images) : null;
        }
        else if (request.Type == ListingType.Restaurant && request.RestaurantDetails != null)
        {
            var r = await _context.Set<RestaurantListingDetails>().FirstOrDefaultAsync(x => x.ListingId == id);
            if (r == null)
            {
                r = new RestaurantListingDetails { Id = Guid.NewGuid(), ListingId = id };
                _context.Set<RestaurantListingDetails>().Add(r);
            }
            r.CuisineType = request.RestaurantDetails.CuisineType;
            r.AverageCost = request.RestaurantDetails.AverageCost;
            r.OpeningHours = request.RestaurantDetails.OpeningHours;
            r.TableCapacity = request.RestaurantDetails.TableCapacity;
            r.TableTypes = request.RestaurantDetails.TableTypes != null ? string.Join(", ", request.RestaurantDetails.TableTypes) : null;
            r.ReservationRules = request.RestaurantDetails.ReservationRules;
            r.Images = request.Images != null ? string.Join(", ", request.Images) : null;
        }
        else if (request.Type == ListingType.Event && request.EventDetails != null)
        {
            var e = await _context.Set<EventListingDetails>().FirstOrDefaultAsync(x => x.ListingId == id);
            if (e == null)
            {
                e = new EventListingDetails { Id = Guid.NewGuid(), ListingId = id };
                _context.Set<EventListingDetails>().Add(e);
            }
            e.EventName = request.EventDetails.EventName;
            e.Organizer = request.EventDetails.Organizer;
            e.DateAndTime = request.EventDetails.DateAndTime;
            e.SeatCount = request.EventDetails.SeatCount;
            e.TicketPrice = request.EventDetails.TicketPrice;
            e.EventType = request.EventDetails.EventType;
            e.VenueName = request.EventDetails.VenueName;
            e.VenueAddress = request.EventDetails.VenueAddress;
            e.TicketTypesJson = request.EventDetails.TicketTypes != null ? System.Text.Json.JsonSerializer.Serialize(request.EventDetails.TicketTypes) : null;
            e.Images = request.Images != null ? string.Join(", ", request.Images) : null;
        }
        else if (request.Type == ListingType.CarRental && request.CarRentalDetails != null)
        {
            var cr = await _context.Set<CarRentalListingDetails>().FirstOrDefaultAsync(x => x.ListingId == id);
            if (cr == null)
            {
                cr = new CarRentalListingDetails { Id = Guid.NewGuid(), ListingId = id };
                _context.Set<CarRentalListingDetails>().Add(cr);
            }
            cr.Brand = request.CarRentalDetails.Brand;
            cr.Model = request.CarRentalDetails.Model;
            cr.Transmission = request.CarRentalDetails.Transmission;
            cr.PricePerDay = request.CarRentalDetails.PricePerDay;
            cr.SeatCount = request.CarRentalDetails.SeatCount;
            cr.FuelType = request.CarRentalDetails.FuelType;
            cr.AvailabilityStatus = request.CarRentalDetails.AvailabilityStatus;
            cr.Year = request.CarRentalDetails.Year;
            cr.HourlyRate = request.CarRentalDetails.HourlyRate;
            cr.PickupLocation = request.CarRentalDetails.PickupLocation;
            cr.ReturnLocation = request.CarRentalDetails.ReturnLocation;
            cr.InsuranceOptions = request.CarRentalDetails.InsuranceOptions;
            cr.Images = request.Images != null ? string.Join(", ", request.Images) : null;
        }
        else if (request.Type == ListingType.Activity && request.ActivityDetails != null)
        {
            var a = await _context.Set<ActivityListingDetails>().FirstOrDefaultAsync(x => x.ListingId == id);
            if (a == null)
            {
                a = new ActivityListingDetails { Id = Guid.NewGuid(), ListingId = id };
                _context.Set<ActivityListingDetails>().Add(a);
            }
            a.ActivityType = request.ActivityDetails.ActivityType;
            a.DurationHours = request.ActivityDetails.DurationHours;
            a.DifficultyLevel = request.ActivityDetails.DifficultyLevel;
            a.Price = request.ActivityDetails.Price;
            a.MinGroupSize = request.ActivityDetails.MinGroupSize;
            a.MaxGroupSize = request.ActivityDetails.MaxGroupSize;
            a.MinAge = request.ActivityDetails.MinAge;
            a.MaxAge = request.ActivityDetails.MaxAge;
            a.IncludedServices = request.ActivityDetails.IncludedServices != null ? string.Join(", ", request.ActivityDetails.IncludedServices) : null;
            a.SafetyRequirements = request.ActivityDetails.SafetyRequirements;
            a.AvailabilitySchedule = request.ActivityDetails.AvailabilitySchedule;
            a.Images = request.Images != null ? string.Join(", ", request.Images) : null;
        }

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