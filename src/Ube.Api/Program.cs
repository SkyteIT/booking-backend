using Ube.Application.Interfaces.Repositories;
using Ube.Application.Services.Admin;
using Ube.Application.Services.Listings;
using Ube.Application.Services.Cart;
using Ube.Infrastructure.Persistence;
using Ube.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Ube.Domain.Entities.Users;
using Ube.Domain.Enums;
using Ube.Domain.Enums.Users;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Vendors;
using Ube.Api.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=UbeDb;Trusted_Connection=True;TrustServerCertificate=True";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = AuthTokenFactory.ValidationParameters(builder.Configuration);
    });

builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IListingRepository, ListingRepository>();
builder.Services.AddScoped<IListingService, ListingService>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var adminEmail = "admin@ube.local";
    if (!await db.Users.AnyAsync(user => user.Email == adminEmail))
    {
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = adminEmail,
            PasswordHash = PasswordHasher.Hash("SecurePassword123!"),
            FirstName = "Super",
            LastName = "Admin",
            IsEmailVerified = true,
            AuthProvider = AuthProvider.Local,
            Role = UserRole.Admin,
        });

        await db.SaveChangesAsync();
    }

    var category = await db.Categories.FirstOrDefaultAsync(item => item.Name == "Activities");
    if (category is null)
    {
        category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Activities",
            Description = "Experiences and activities",
            Type = "Activity",
            IsActive = true,
        };
        db.Categories.Add(category);
    }
    else if (string.IsNullOrWhiteSpace(category.Type))
    {
        category.Type = "Activity";
        await db.SaveChangesAsync();
    }

    var legacyListings = await db.Listings
        .Where(listing => listing.ListingType == "" || listing.ImagesJson == "" || listing.TagsJson == "")
        .ToListAsync();
    if (legacyListings.Count > 0)
    {
        foreach (var listing in legacyListings)
        {
            if (string.IsNullOrWhiteSpace(listing.ListingType)) listing.ListingType = category.Type ?? "Activity";
            if (string.IsNullOrWhiteSpace(listing.ImagesJson)) listing.ImagesJson = "[]";
            if (string.IsNullOrWhiteSpace(listing.TagsJson)) listing.TagsJson = "[]";
        }
        await db.SaveChangesAsync();
    }

    var vendorUser = await db.Users.FirstOrDefaultAsync(user => user.Email == "vendor@ube.local");
    if (vendorUser is null)
    {
        vendorUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "vendor@ube.local",
            PasswordHash = PasswordHasher.Hash("SecurePassword123!"),
            FirstName = "Ube",
            LastName = "Experiences",
            IsEmailVerified = true,
            AuthProvider = AuthProvider.Local,
            Role = UserRole.Vendor,
        };
        db.Users.Add(vendorUser);
        await db.SaveChangesAsync();
    }

    var vendorProfile = await db.VendorProfiles.FirstOrDefaultAsync(profile => profile.UserId == vendorUser.Id);
    if (vendorProfile is null)
    {
        vendorProfile = new VendorProfile
        {
            Id = Guid.NewGuid(),
            UserId = vendorUser.Id,
            BusinessName = "Ube Experiences",
            BusinessType = "Activities",
            Description = "Local experiences for every traveler.",
            ContactNumber = "0112345678",
            IsActive = true,
        };
        db.VendorProfiles.Add(vendorProfile);
        await db.SaveChangesAsync();
    }

    if (!await db.Listings.AnyAsync())
    {
        db.Listings.AddRange(
            new Listing
            {
                Id = Guid.NewGuid(), VendorProfileId = vendorProfile.Id, CategoryId = category.Id,
                Title = "Colombo Sunset City Tour", Description = "A guided evening tour through Colombo.",
                Price = 8000, Currency = "LKR", Location = "Colombo", IsActive = true,
            },
            new Listing
            {
                Id = Guid.NewGuid(), VendorProfileId = vendorProfile.Id, CategoryId = category.Id,
                Title = "Kandy Highland Experience", Description = "A full-day cultural and scenic experience.",
                Price = 12500, Currency = "LKR", Location = "Kandy", IsActive = true,
            });
        await db.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
