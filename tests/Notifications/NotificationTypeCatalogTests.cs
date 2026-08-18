using Ube.Domain.Enums.Notifications;

namespace Ube.Tests.Notifications;

public class NotificationTypeCatalogTests
{
    [Fact]
    public void Catalog_Contains_Expected_Group_Sizes_And_Unique_Values()
    {
        Assert.Equal(14, NotificationTypeCatalog.Admin.Length);
        Assert.Equal(19, NotificationTypeCatalog.Vendor.Length);
        Assert.Equal(20, NotificationTypeCatalog.Customer.Length);

        var all = NotificationTypeCatalog.All.ToArray();

        Assert.Equal(NotificationTypeCatalog.Legacy.Length
            + NotificationTypeCatalog.Admin.Length
            + NotificationTypeCatalog.Vendor.Length
            + NotificationTypeCatalog.Customer.Length, all.Length);

        Assert.Equal(all.Length, all.Distinct().Count());
    }

    [Theory]
    [MemberData(nameof(ExpectedAdminTypes))]
    public void Admin_Group_Contains_Expected_Types(NotificationType type)
        => Assert.Contains(type, NotificationTypeCatalog.Admin);

    [Theory]
    [MemberData(nameof(ExpectedVendorTypes))]
    public void Vendor_Group_Contains_Expected_Types(NotificationType type)
        => Assert.Contains(type, NotificationTypeCatalog.Vendor);

    [Theory]
    [MemberData(nameof(ExpectedCustomerTypes))]
    public void Customer_Group_Contains_Expected_Types(NotificationType type)
        => Assert.Contains(type, NotificationTypeCatalog.Customer);

    public static IEnumerable<object[]> ExpectedAdminTypes()
    {
        yield return new object[] { NotificationType.AdminNewVendorRegistered };
        yield return new object[] { NotificationType.AdminNewListingSubmitted };
        yield return new object[] { NotificationType.AdminVendorRequestApproval };
        yield return new object[] { NotificationType.AdminNewBookingCreated };
        yield return new object[] { NotificationType.AdminBookingCancellation };
        yield return new object[] { NotificationType.AdminPaymentIssue };
        yield return new object[] { NotificationType.AdminCustomerReportSubmitted };
        yield return new object[] { NotificationType.AdminListingReported };
        yield return new object[] { NotificationType.AdminVendorAccountSuspended };
        yield return new object[] { NotificationType.AdminListingApprovalRequired };
        yield return new object[] { NotificationType.AdminListingUpdateRequiresReview };
        yield return new object[] { NotificationType.AdminNewCustomerRegistration };
        yield return new object[] { NotificationType.AdminRefundRequest };
        yield return new object[] { NotificationType.AdminSystemAlert };
    }

    public static IEnumerable<object[]> ExpectedVendorTypes()
    {
        yield return new object[] { NotificationType.VendorListingsApproved };
        yield return new object[] { NotificationType.VendorListingsRejected };
        yield return new object[] { NotificationType.VendorListingsUpdateApproved };
        yield return new object[] { NotificationType.VendorListingsUpdateRejected };
        yield return new object[] { NotificationType.VendorNewBookingReceived };
        yield return new object[] { NotificationType.VendorBookingConfirmed };
        yield return new object[] { NotificationType.VendorBookingCancelledByCustomer };
        yield return new object[] { NotificationType.VendorBookingRescheduled };
        yield return new object[] { NotificationType.VendorPaymentReceived };
        yield return new object[] { NotificationType.VendorPaymentFailed };
        yield return new object[] { NotificationType.VendorRefundRequested };
        yield return new object[] { NotificationType.VendorRefundProcessed };
        yield return new object[] { NotificationType.VendorNewCustomerReview };
        yield return new object[] { NotificationType.VendorLowRatingReceived };
        yield return new object[] { NotificationType.VendorAccountApproved };
        yield return new object[] { NotificationType.VendorAccountRejected };
        yield return new object[] { NotificationType.VendorAccountSuspended };
        yield return new object[] { NotificationType.VendorListingExpired };
        yield return new object[] { NotificationType.VendorSystemAnnouncement };
    }

    public static IEnumerable<object[]> ExpectedCustomerTypes()
    {
        yield return new object[] { NotificationType.CustomerBookingConfirmed };
        yield return new object[] { NotificationType.CustomerBookingPending };
        yield return new object[] { NotificationType.CustomerBookingRejected };
        yield return new object[] { NotificationType.CustomerBookingCancelledByVendor };
        yield return new object[] { NotificationType.CustomerBookingCancelledByCustomer };
        yield return new object[] { NotificationType.CustomerBookingRescheduled };
        yield return new object[] { NotificationType.CustomerBookingReminder };
        yield return new object[] { NotificationType.CustomerPaymentSuccessful };
        yield return new object[] { NotificationType.CustomerPaymentFailed };
        yield return new object[] { NotificationType.CustomerRefundRequested };
        yield return new object[] { NotificationType.CustomerRefundApproved };
        yield return new object[] { NotificationType.CustomerRefundRejected };
        yield return new object[] { NotificationType.CustomerRefundProcessed };
        yield return new object[] { NotificationType.CustomerBookingCompleted };
        yield return new object[] { NotificationType.CustomerReviewReminder };
        yield return new object[] { NotificationType.CustomerReviewSubmitted };
        yield return new object[] { NotificationType.CustomerBookingStatusChanged };
        yield return new object[] { NotificationType.CustomerAccountVerification };
        yield return new object[] { NotificationType.CustomerSystemAnnouncement };
        yield return new object[] { NotificationType.CustomerPromotionAndOfferAvailable };
    }
}
