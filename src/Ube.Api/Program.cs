using Microsoft.EntityFrameworkCore;
using Ube.Infrastructure.Persistence;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Interfaces.Services;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Dashboard;
using Ube.Application.Features.Bookings;
using Ube.Infrastructure.Persistence.Repositories.Bookings;
using Ube.Infrastructure.Persistence.Repositories.Listings;
using Ube.Infrastructure.Persistence.Seed;
using System.Text.Json.Serialization;
using Ube.Application.Features.Availability;
using Ube.Application.Features.Availability.Strategies;
using Ube.Application.Features.Listings;
using FluentValidation;
using FluentValidation.AspNetCore;
using Ube.Infrastructure.Persistence.Repositories.Users;
using Ube.Infrastructure.Persistence.Repositories.Vendors;
using Ube.Application.Features.Vendors;
using Ube.Application.Common.Helpers;
using Ube.Infrastructure.Services.Auth;
using Ube.Api.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using Ube.Domain.Entities.Users;
using Ube.Application.Features.Vendors.Payout;
using Ube.Application.Features.VendorRegistration;
using Ube.Application.Features.Localization;
using Ube.Application.Features.Admin.VendorApplications;
using Ube.Application.Features.Reviews;
using Ube.Infrastructure.Persistence.Repositories.Reviews;
using Ube.Application.Common.Models;
using Ube.Application.Features.Notifications.Email;
using Ube.Application.Features.Auth;
using Ube.Application.Features.Security;
using Ube.Infrastructure.Persistence.Repositories.Auth;
using Microsoft.AspNetCore.RateLimiting;
using Ube.Application.Features.Content;
using Ube.Application.Features.Notifications;
using Ube.Application.Interfaces;
using Ube.Application.Services;
using Ube.Infrastructure.Persistence.Repositories.Content;
using Ube.Infrastructure.Persistence.Repositories.Notifications;
using Ube.Infrastructure.Services;


var builder = WebApplication.CreateBuilder(args);

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Add auth service and token service
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
// Register validators from the auth DTO assembly
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestDto>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)).UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpContextAccessor();


// Register repositories and services

builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IListingRepository, ListingRepository>();
builder.Services.AddScoped<RatingHelper>();
builder.Services.AddScoped<IBlockedDateRepository, BlockedDateRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IDashboardService, VendorDashboardService>();
builder.Services.AddScoped<IListingService, ListingService>();
// Register availability strategies
builder.Services.AddScoped<IAvailabilityStrategy, CapacityStrategy>();
builder.Services.AddScoped<IAvailabilityStrategy, SingleUnitStrategy>();
builder.Services.AddScoped<StrategySelector>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();

// register repositories for user and vendor profiles
builder.Services.AddScoped<IVendorProfileService, VendorProfileService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVendorProfileRepository, VendorProfileRepository>();
builder.Services.AddScoped<IVendorPayoutRepository, VendorPayoutRepository>();
builder.Services.AddScoped<IVendorApplicationRepository, VendorApplicationRepository>();
builder.Services.AddScoped<IVendorRegistrationService, VendorRegistrationService>();
builder.Services.AddScoped<IVendorPayoutService, VendorPayoutService>();
builder.Services.AddScoped<ILocalizationRepository, LocalizationRepository>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();

// Admin services
builder.Services.AddScoped<IAdminVendorApplicationService, AdminVendorApplicationService>();

//add Unit of work (handling transactions in complex operations)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
//add Review service
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
//add email verification token repository
builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Wire ApplicationDbContext as IAppDbContext for content/search services
builder.Services.AddScoped<IAppDbContext>(p => p.GetRequiredService<ApplicationDbContext>());
// Content & notification repositories
builder.Services.AddScoped<IBannerRepository, BannerRepository>();
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
// Content & search services (from merged branch)
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IBannerService, BannerService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISmsService, SmsService>();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", o =>
    {
        o.PermitLimit = 5;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}" });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowCredentials()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
var app = builder.Build();
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await TestDataSeeder.SeedAsync(dbContext, app.Logger);
}
app.UseMiddleware<Ube.Api.Middleware.ExceptionMiddleware>();
app.UseRateLimiter();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();


app.MapControllers();

app.Run();
