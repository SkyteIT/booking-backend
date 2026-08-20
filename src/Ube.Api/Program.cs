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
using Ube.Application.Features.Questions;
using Ube.Application.Features.Reviews;
using Ube.Infrastructure.Persistence.Repositories.Questions;
using Ube.Infrastructure.Persistence.Repositories.Reviews;
using Ube.Application.Common.Models;
using Ube.Application.Features.Notifications.Email;
using Ube.Application.Features.Auth;
using Ube.Application.Features.Security;
using Ube.Infrastructure.Persistence.Repositories.Auth;
using Microsoft.AspNetCore.RateLimiting;
using Ube.Application.Features.Content.Banner;
using Ube.Application.Features.Content.Category;
using Ube.Application.Features.Content.Promotion;
using Ube.Application.Features.Notifications;
using Ube.Application.Features.Search;
using Ube.Infrastructure.Persistence.Repositories.Content;
using Ube.Infrastructure.Persistence.Repositories.Notifications;
using Ube.Infrastructure.Integrations.Smtp;
using Ube.Infrastructure.Integrations.Sms;
using Ube.Infrastructure.Integrations.Push;
using Ube.Infrastructure.Services;
using Ube.Application.Features.Cart;
using Ube.Infrastructure.Persistence.Repositories.Cart;
using Ube.Application.Features.Admin.Dashboard;
using Ube.Application.Features.Users;
using Ube.Infrastructure.Persistence.Repositories.Admin;
using Ube.Application.Features.Payments;
using Ube.Infrastructure.Persistence.Repositories.Payments;
using Ube.Infrastructure.Integrations.PaymentGateway;
using Ube.Application.Features.Fraud;
using Ube.Infrastructure.Persistence.Repositories.Fraud;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Ube.Api.Hubs;
using Ube.Api.Services;


var builder = WebApplication.CreateBuilder(args);
// (KeyVault:Uri) so each environment can point at its own vault without a code change.
var keyVaultUriSetting = builder.Configuration["KeyVault:Uri"];
if (!string.IsNullOrWhiteSpace(keyVaultUriSetting))
{
    var credential = new ClientSecretCredential(
        builder.Configuration["Azure:TenantId"],
        builder.Configuration["Azure:ClientId"],
        builder.Configuration["Azure:ClientSecret"]
    );
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUriSetting), credential);
}
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
builder.Services.AddScoped<IListingUnitRepository, ListingUnitRepository>();
builder.Services.AddScoped<IListingUnitService, ListingUnitService>();
builder.Services.AddScoped<ISeasonalPricingRepository, SeasonalPricingRepository>();
builder.Services.AddScoped<ISeasonalPricingService, SeasonalPricingService>();
builder.Services.AddScoped<IListingOfferRepository, ListingOfferRepository>();
builder.Services.AddScoped<IListingOfferService, ListingOfferService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
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
builder.Services.AddScoped<IListingQuestionService, ListingQuestionService>();
builder.Services.AddScoped<IListingQuestionRepository, ListingQuestionRepository>();
//add email verification token repository
builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
builder.Services.AddScoped<ITwoFactorChallengeRepository, TwoFactorChallengeRepository>();
builder.Services.AddScoped<ITwoFactorBackupCodeRepository, TwoFactorBackupCodeRepository>();
builder.Services.AddScoped<ITrustedDeviceRepository, TrustedDeviceRepository>();
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
builder.Services.AddScoped<Ube.Application.Features.Notifications.IAdminAlertService, Ube.Application.Features.Notifications.AdminAlertService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IPushService, PushService>();
builder.Services.AddScoped<Ube.Application.Features.Support.ISupportService, Ube.Application.Features.Support.SupportService>();

// Cart
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartService>();

// Admin dashboard
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IRoleChangeRequestRepository, RoleChangeRequestRepository>();
builder.Services.AddScoped<IRoleChangeRequestService, RoleChangeRequestService>();
builder.Services.AddScoped<Ube.Application.Common.Interfaces.Persistence.IEmailChangeRequestRepository, Ube.Infrastructure.Persistence.Repositories.Users.EmailChangeRequestRepository>();
builder.Services.AddScoped<IEmailChangeRequestService, EmailChangeRequestService>();

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IRefundRepository, RefundRepository>();
builder.Services.AddScoped<ILedgerRepository, LedgerRepository>();
builder.Services.AddScoped<IVendorRevenueReportService, VendorRevenueReportService>();
builder.Services.AddScoped<IPayoutBatchRepository, PayoutBatchRepository>();
builder.Services.AddScoped<IVendorCommissionRepository, VendorCommissionRepository>();
builder.Services.AddScoped<IPaymentAuditLogRepository, PaymentAuditLogRepository>();
builder.Services.AddScoped<ICommissionResolverService, CommissionResolverService>();
builder.Services.AddScoped<IPaymentGatewayClient, MockPaymentGatewayClient>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IRefundService, RefundService>();
builder.Services.AddScoped<IPayoutBatchService, PayoutBatchService>();
builder.Services.AddScoped<IVendorInvoiceRepository, VendorInvoiceRepository>();
builder.Services.AddScoped<IVendorInvoiceService, VendorInvoiceService>();
builder.Services.AddScoped<IPayoutExportRepository, PayoutExportRepository>();
builder.Services.AddScoped<IPayoutExportSettingsRepository, PayoutExportSettingsRepository>();
builder.Services.AddScoped<IPayoutExportService, PayoutExportService>();
builder.Services.AddScoped<IPaymentDisputeRepository, PaymentDisputeRepository>();
builder.Services.AddScoped<IPaymentDisputeService, PaymentDisputeService>();
builder.Services.AddScoped<ICommissionPolicyService, CommissionPolicyService>();
builder.Services.AddScoped<IVendorAdvanceService, VendorAdvanceService>();
builder.Services.AddScoped<IVendorCommissionAcknowledgementService, VendorCommissionAcknowledgementService>();
builder.Services.AddScoped<IPaymentReconciliationService, PaymentReconciliationService>();
builder.Services.Configure<PaymentSchedulerOptions>(builder.Configuration.GetSection("PaymentScheduler"));
builder.Services.AddHostedService<PaymentSchedulerBackgroundService>();
builder.Services.Configure<BookingCompletionOptions>(builder.Configuration.GetSection("BookingCompletion"));
builder.Services.AddHostedService<BookingCompletionBackgroundService>();

// Fraud detection
builder.Services.AddScoped<IFraudFlagRepository, FraudFlagRepository>();
builder.Services.AddScoped<IFraudDetectionService, FraudDetectionService>();
builder.Services.Configure<FraudDetectionOptions>(builder.Configuration.GetSection("FraudDetection"));

// Realtime dashboard/notification push (SignalR)
builder.Services.AddScoped<IRealtimeUpdateService, SignalRRealtimeUpdateService>();
builder.Services.AddSignalR();

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


app.MapHub<RealtimeUpdatesHub>("/hubs/updates");
app.MapControllers();

app.Run();
