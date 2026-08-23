namespace Ube.Application.Common.Helpers;


public static class BusinessDate
{
    private static readonly TimeSpan Offset = TimeSpan.FromHours(5.5);

    public static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow + Offset);
}
