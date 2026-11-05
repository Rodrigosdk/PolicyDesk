using Bogus;

namespace CommonTestUtilities.Requests;

public static class PhoneGenerator
{
    private static readonly string[] AreaCodes = ["11", "21", "31", "41", "51", "61", "71", "81", "83", "85", "91"];

    public static string Generate(Faker faker) =>
        $"({faker.PickRandom(AreaCodes)}) 9 {faker.Random.ReplaceNumbers("####-####")}";
}
