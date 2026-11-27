namespace Logging;

public static class LogDates
{
    public static readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
    public static DateTime Today => ToLocal(DateTime.UtcNow).Date;

    public static DateTime ToLocal(DateTime utc)
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), TimeZone);

    public static (DateTime From, DateTime To) UtcRange(DateTime from, DateTime to)
    {
        if (from.Date > to.Date) throw new ArgumentException("Başlangıç tarihi bitiş tarihinden sonra olamaz.");
        return (TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(from.Date, DateTimeKind.Unspecified), TimeZone),
            TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(to.Date.AddDays(1), DateTimeKind.Unspecified), TimeZone));
    }
}
