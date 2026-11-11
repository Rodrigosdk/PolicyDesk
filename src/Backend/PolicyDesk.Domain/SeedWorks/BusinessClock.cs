namespace PolicyDesk.Domain.SeedWorks;

public sealed class BusinessClock(TimeProvider timeProvider, TimeZoneInfo timeZone)
{
    public DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), timeZone).DateTime);
}
