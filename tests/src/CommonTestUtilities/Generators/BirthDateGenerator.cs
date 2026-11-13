using Bogus;
using CommonTestUtilities.Time;

namespace CommonTestUtilities.Generators;

public static class BirthDateGenerator
{
    public static DateOnly Generate(Faker faker) =>
        faker.Date.PastDateOnly(40, BusinessClockBuilder.DefaultToday.AddYears(-18));
}
