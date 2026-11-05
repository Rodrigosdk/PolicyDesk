using Bogus;

namespace CommonTestUtilities.Requests;

public static class BirthDateGenerator
{
    // Kind Unspecified, como o JSON desserializa e como o Npgsql aceita para persistir
    public static DateTime Generate(Faker faker)
    {
        var date = faker.Date.Past(40, DateTime.Today.AddYears(-18));
        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified);
    }
}
