using Bogus;

namespace CommonTestUtilities.Generators;

public static class PasswordGenerator
{
    public static string Generate() =>
        new Faker().Internet.Password(length: 12, prefix: "Aa1!");
}
