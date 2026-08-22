namespace Ube.Application.Features.Listings.Validators;

internal static class ListingTimeFormat
{
    public const string NumericTimePattern = @"^(?:0?[1-9]|1[0-2]):[0-5][0-9]$";
    public const string PeriodPattern = @"^(?:AM|PM)$";
    // Examples: 9:00 AM, 09:00 PM. Minutes and the AM/PM suffix are required.
    public const string TimePattern = @"^(?:0?[1-9]|1[0-2]):[0-5][0-9] (?:AM|PM)$";

    // Restaurant hours may be a single time or an opening/closing range.
    // Examples: 9:00 AM - 10:30 PM, 11:00 AM.
    public const string TimeOrRangePattern =
        @"^(?:0?[1-9]|1[0-2]):[0-5][0-9] (?:AM|PM)(?:\s*-\s*(?:0?[1-9]|1[0-2]):[0-5][0-9] (?:AM|PM))?$";

    public static (string? Time, string? Period) ParseTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return (null, null);

        var formats = new[] { "h:mm tt", "hh:mm tt", "H:mm", "HH:mm" };
        if (!DateTime.TryParseExact(value.Trim(), formats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var parsed))
            return (null, null);

        return (parsed.ToString("h:mm", System.Globalization.CultureInfo.InvariantCulture),
            parsed.ToString("tt", System.Globalization.CultureInfo.InvariantCulture));
    }

    public static (string? OpeningTime, string? OpeningPeriod, string? ClosingTime, string? ClosingPeriod)
        ParseRange(string? value)
    {
        var parts = value?.Split('-', 2, StringSplitOptions.TrimEntries);
        var opening = ParseTime(parts?.ElementAtOrDefault(0));
        var closing = ParseTime(parts?.ElementAtOrDefault(1));
        return (opening.Time, opening.Period, closing.Time, closing.Period);
    }
}
