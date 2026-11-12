using Bogus;
using CommonTestUtilities.Requests;
using PolicyDesk.Domain.Entities;

namespace CommonTestUtilities.Entities;

public static class UserBuilder
{
    public static User Build(
        string? name = null,
        string? email = null,
        string? password = null)
    {
        var faker = new Faker();
        return new User(
            name ?? faker.Name.FullName(),
            email ?? faker.Internet.Email(),
            password ?? PasswordGenerator.Generate());
    }
}
