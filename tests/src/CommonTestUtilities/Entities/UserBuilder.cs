using Bogus;
using PolicyDesk.Domain.Entities;

namespace CommonTestUtilities.Entities;

public static class UserBuilder
{
    public static User Build(
        string? name = null,
        string? email = null,
        string? password = null) =>
        new Faker<User>()
            .CustomInstantiator(_ => (User)Activator.CreateInstance(typeof(User), nonPublic: true)!)
            .RuleFor(user => user.Name, f => name ?? f.Person.FullName)
            .RuleFor(user => user.Email, f => email ?? f.Internet.Email(f.Person.FirstName, f.Person.LastName).ToLowerInvariant())
            .RuleFor(user => user.Password, f => password ?? f.Internet.Password(length: 12, prefix: "Aa1!"))
            .Generate();
}
