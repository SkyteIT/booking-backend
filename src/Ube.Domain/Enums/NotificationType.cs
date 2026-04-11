namespace Ube.Domain.Enums;

public enum NotificationType
{
	NewBookingRequest = 1,
	BookingConfirmation = 2,
	Cancellation = 3,
	PaymentReceived = 4,
	PayoutProcessed = 5,
	PaymentFailed = 6,
	NewReview = 7,
	ReviewResponse = 8,
	SecurityAlert = 9,
	AccountUpdate = 10,
	VendorApproved = 11,
	RefundPending = 12,
	SystemMaintenance = 13,
	GatewayError = 14,
	RevenueTargetAchieved = 15
}
