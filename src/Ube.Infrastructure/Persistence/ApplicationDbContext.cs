using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Auth;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Carts;
using Ube.Domain.Entities.Content;
using Ube.Domain.Entities.Fraud;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Notifications;
using Ube.Domain.Entities.Payments;
using Ube.Domain.Entities.Questions;
using Ube.Domain.Entities.Reviews;
using Ube.Domain.Entities.Users;
using Ube.Domain.Entities.Vendors;

namespace Ube.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IAppDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {}

    // ================= BASE TABLES =================
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<Listing> Listings { get; set; } = default!;
    public DbSet<Booking> Bookings { get; set; } = default!;
    public DbSet<Review> Reviews { get; set; } = default!;
    public DbSet<ReviewLike> ReviewLikes { get; set; } = default!;
    public DbSet<ListingQuestion> ListingQuestions { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<VendorApplication> VendorApplications { get; set; } = default!;
    public DbSet<VendorProfile> VendorProfiles { get; set; } = default!;
    public DbSet<VendorPayout> VendorPayouts { get; set; } = default!;
    public DbSet<BlockedDate> BlockedDates { get; set; } = default!;
    public DbSet<UserLocalizationSettings> UserLocalizationSettings { get; set; } = default!;
    public DbSet<RoleChangeRequest> RoleChangeRequests { get; set; } = default!;
    public DbSet<EmailChangeRequest> EmailChangeRequests { get; set; } = default!;
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; } = default!;
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = default!;
    public DbSet<TwoFactorChallenge> TwoFactorChallenges { get; set; } = default!;
    public DbSet<TwoFactorBackupCode> TwoFactorBackupCodes { get; set; } = default!;
    public DbSet<TrustedDevice> TrustedDevices { get; set; } = default!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;
    public DbSet<Banner> Banners { get; set; } = default!;
    public DbSet<Promotion> Promotions { get; set; } = default!;
    public DbSet<Notification> Notifications { get; set; } = default!;
    public DbSet<NotificationPreference> NotificationPreferences { get; set; } = default!;
    public DbSet<PushSubscription> PushSubscriptions { get; set; } = default!;
    public DbSet<ListingImage> ListingImages { get; set; } = default!;
    public DbSet<ListingUnit> ListingUnits { get; set; } = default!;
    public DbSet<ListingOptionGroup> ListingOptionGroups { get; set; } = default!;
    public DbSet<ListingOptionValue> ListingOptionValues { get; set; } = default!;
    public DbSet<SeasonalPricingRule> SeasonalPricingRules { get; set; } = default!;
    public DbSet<ListingOffer> ListingOffers { get; set; } = default!;
    public DbSet<Cart> Carts { get; set; } = default!;
    public DbSet<CartItem> CartItems { get; set; } = default!;

    // ================= LISTING DETAIL TABLES =================
    public DbSet<HotelListingDetails> HotelListingDetails { get; set; } = default!;
    public DbSet<RestaurantListingDetails> RestaurantListingDetails { get; set; } = default!;
    public DbSet<EventListingDetails> EventListingDetails { get; set; } = default!;
    public DbSet<CarRentalListingDetails> CarRentalListingDetails { get; set; } = default!;
    public DbSet<ActivityListingDetails> ActivityListingDetails { get; set; } = default!;

    // ================= CUSTOM FIELDS =================
    public DbSet<CategoryCustomField> CategoryCustomFields { get; set; } = default!;
    public DbSet<ListingCustomFieldValue> ListingCustomFieldValues { get; set; } = default!;

    // ================= PAYMENTS =================
    public DbSet<Payment> Payments { get; set; } = default!;
    public DbSet<Refund> Refunds { get; set; } = default!;
    public DbSet<LedgerEntry> LedgerEntries { get; set; } = default!;
    public DbSet<PayoutBatch> PayoutBatches { get; set; } = default!;
    public DbSet<PaymentAuditLogEntry> PaymentAuditLogEntries { get; set; } = default!;
    public DbSet<VendorCommissionOverride> VendorCommissionOverrides { get; set; } = default!;
    public DbSet<VendorCommissionAcknowledgement> VendorCommissionAcknowledgements { get; set; } = default!;
    public DbSet<LoyaltyDiscountTier> LoyaltyDiscountTiers { get; set; } = default!;
    public DbSet<VendorCommissionInvoice> VendorCommissionInvoices { get; set; } = default!;
    public DbSet<PayoutExportRun> PayoutExportRuns { get; set; } = default!;
    public DbSet<PayoutExportSettings> PayoutExportSettingsRows { get; set; } = default!;
    public DbSet<PaymentDispute> PaymentDisputes { get; set; } = default!;

    // ================= FRAUD =================
    public DbSet<FraudFlag> FraudFlags { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the current assembly (Configurations directory)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
