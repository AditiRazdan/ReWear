namespace LocalBakery.Utilities;

public static class TimeHelper
{
    private const int SgOffsetHours = 8;

    public static DateTime GetSgtNow() => DateTime.UtcNow.AddHours(SgOffsetHours);

    public static DateTime GetSgtDate() => GetSgtNow().Date;

    public static DateTime ToUtc(DateTime sgtDateTime)
    {
        var unspecified = DateTime.SpecifyKind(sgtDateTime, DateTimeKind.Utc);
        return unspecified.AddHours(-SgOffsetHours);
    }

    public static DateTime ToSgt(DateTime utcDateTime)
    {
        return DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc).AddHours(SgOffsetHours);
    }
}
