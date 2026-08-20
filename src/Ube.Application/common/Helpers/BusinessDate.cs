namespace Ube.Application.Common.Helpers;

// "Today" for calendar-day comparisons that are meant to match what a
// human vendor/customer sees on their own calendar - offer/promotion
// active windows, "starts today" checks, etc. - as opposed to comparisons
// that are genuinely about elapsed UTC time (token expiry, timestamps).
//
// DateTime.UtcNow crosses into the next day several hours before Sri
// Lanka's local clock does (UTC+5:30), so a vendor who picks "today" as
// an offer's start date in their own browser can have that offer judged
// "not started yet" by the server for the first ~5.5 hours of their own
// day. Every such calendar-day check should go through here instead of
// calling DateOnly.FromDateTime(DateTime.UtcNow) directly, so they all
// agree on the same "today" - fixing one and leaving the others on raw
// UTC would just move the bug around.
public static class BusinessDate
{
    // UBE's users are Sri Lanka based (see the "Asia/Colombo" default in
    // TestDataSeeder's UserLocalizationSettings) - a fixed +5:30 offset,
    // not a full IANA TimeZoneInfo lookup, since there's no per-vendor
    // timezone stored to look up yet.
    private static readonly TimeSpan Offset = TimeSpan.FromHours(5.5);

    public static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow + Offset);
}
