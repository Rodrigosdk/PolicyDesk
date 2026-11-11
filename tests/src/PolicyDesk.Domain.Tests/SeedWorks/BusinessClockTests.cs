using CommonTestUtilities.Time;
using Shouldly;

namespace PolicyDesk.Domain.Tests.SeedWorks;

public class BusinessClockTests
{
    [Theory(DisplayName = nameof(Deve_RetornarODiaDeBrasilia_Quando_HorarioEmUtcEstiverPertoDaMeiaNoite))]
    [Trait("Domain", "Relógio de negócio")]
    [InlineData(0, 0, 10)]
    [InlineData(1, 0, 10)]
    [InlineData(2, 59, 10)]
    [InlineData(3, 0, 11)]
    public void Deve_RetornarODiaDeBrasilia_Quando_HorarioEmUtcEstiverPertoDaMeiaNoite(int utcHour, int utcMinute, int expectedDay)
    {
        var clock = BusinessClockBuilder.Build(new DateTimeOffset(2026, 3, 11, utcHour, utcMinute, 0, TimeSpan.Zero));

        clock.Today.ShouldBe(new DateOnly(2026, 3, expectedDay));
    }

    [Fact(DisplayName = nameof(Deve_RetornarOInstanteEmUtc_Quando_ConsultarAHoraAtual))]
    [Trait("Domain", "Relógio de negócio")]
    public void Deve_RetornarOInstanteEmUtc_Quando_ConsultarAHoraAtual()
    {
        var clock = BusinessClockBuilder.Build(BusinessClockBuilder.LateEveningUtcNow);

        clock.UtcNow.ShouldBe(new DateTime(2026, 3, 11, 1, 0, 0, DateTimeKind.Utc));
        clock.UtcNow.Kind.ShouldBe(DateTimeKind.Utc);
    }
}
