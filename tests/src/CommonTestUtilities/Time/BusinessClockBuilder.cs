using Microsoft.Extensions.Time.Testing;
using PolicyDesk.Domain.SeedWorks;

namespace CommonTestUtilities.Time;

public static class BusinessClockBuilder
{
    public static readonly DateTimeOffset DefaultUtcNow = new(2026, 3, 10, 15, 0, 0, TimeSpan.Zero);
    public static readonly DateOnly DefaultToday = new(2026, 3, 10);
    public static readonly DateTimeOffset LateEveningUtcNow = new(2026, 3, 11, 1, 0, 0, TimeSpan.Zero);
    public static readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public static BusinessClock Build(DateTimeOffset? utcNow = null) =>
        new(new FakeTimeProvider(utcNow ?? DefaultUtcNow), TimeZone);
}
