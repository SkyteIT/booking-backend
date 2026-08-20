namespace Ube.Domain.Enums.Notifications;

public static class NotificationTypeCatalog
{
    public static readonly NotificationType[] Admin =
    [
        NotificationType.AdminNewVendorRegistered,
        NotificationType.AdminNewListingSubmitted,
        NotificationType.AdminVendorRequestApproval,
        NotificationType.AdminNewBookingCreated,
        NotificationType.AdminBookingCancellation,
        NotificationType.AdminPaymentIssue,
        NotificationType.AdminCustomerReportSubmitted,
        NotificationType.AdminListingReported,
        NotificationType.AdminVendorAccountSuspended,
        NotificationType.AdminListingApprovalRequired,
        NotificationType.AdminListingUpdateRequiresReview,
        NotificationType.AdminNewCustomerRegistration,
        NotificationType.AdminRefundRequest,
        NotificationType.AdminSystemAlert
    ];

    public static readonly NotificationType[] Vendor =
    [
        NotificationType.VendorListingsApproved,
        NotificationType.VendorListingsRejected,
        NotificationType.VendorListingsUpdateApproved,
        NotificationType.VendorListingsUpdateRejected,
        NotificationType.VendorNewBookingReceived,
        NotificationType.VendorBookingConfirmed,
        NotificationType.VendorBookingCancelledByCustomer,
        NotificationType.VendorBookingRescheduled,
        NotificationType.VendorPaymentReceived,
        NotificationType.VendorPaymentFailed,
        NotificationType.VendorRefundRequested,
        NotificationType.VendorRefundProcessed,
        NotificationType.VendorNewCustomerReview,
        NotificationType.VendorLowRatingReceived,
        NotificationType.VendorAccountApproved,
        NotificationType.VendorAccountRejected,
        NotificationType.VendorAccountSuspended,
        NotificationType.VendorListingExpired,
        NotificationType.VendorSystemAnnouncement
    ];

    public static readonly NotificationType[] Customer =
    [
        NotificationType.CustomerBookingConfirmed,
        NotificationType.CustomerBookingPending,
        NotificationType.CustomerBookingRejected,
        NotificationType.CustomerBookingCancelledByVendor,
        NotificationType.CustomerBookingCancelledByCustomer,
        NotificationType.CustomerBookingRescheduled,
        NotificationType.CustomerBookingReminder,
        NotificationType.CustomerPaymentSuccessful,
        NotificationType.CustomerPaymentFailed,
        NotificationType.CustomerRefundRequested,
        NotificationType.CustomerRefundApproved,
        NotificationType.CustomerRefundRejected,
        NotificationType.CustomerRefundProcessed,
        NotificationType.CustomerBookingCompleted,
        NotificationType.CustomerReviewReminder,
        NotificationType.CustomerReviewSubmitted,
        NotificationType.CustomerBookingStatusChanged,
        NotificationType.CustomerAccountVerification,
        NotificationType.CustomerSystemAnnouncement,
        NotificationType.CustomerPromotionAndOfferAvailable
    ];

    public static readonly NotificationType[] Legacy =
    [
        NotificationType.NewBookingRequest,
        NotificationType.BookingConfirmation,
        NotificationType.Cancellation,
        NotificationType.PaymentReceived,
        NotificationType.PayoutProcessed,
        NotificationType.PaymentFailed,
        NotificationType.NewReview,
        NotificationType.ReviewResponse,
        NotificationType.SecurityAlert,
        NotificationType.AccountUpdate,
        NotificationType.VendorApproved,
        NotificationType.RefundPending,
        NotificationType.SystemMaintenance,
        NotificationType.GatewayError,
        NotificationType.RevenueTargetAchieved
    ];

    public static IReadOnlyCollection<NotificationType> All =>
        Legacy.Concat(Admin).Concat(Vendor).Concat(Customer).ToArray();
}
