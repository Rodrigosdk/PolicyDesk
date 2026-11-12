using Bogus;
using PolicyDesk.Communication.Requests;

namespace CommonTestUtilities.Requests;

public static class RequestUserJsonBuilder
{
    public static RequestUserJson Build(
        string? name = null,
        string? email = null,
        string? password = null)
    {
        var faker = new Faker();
        return new RequestUserJson
        {
            Nome = name ?? faker.Name.FullName(),
            Email = email ?? faker.Internet.Email(),
            Senha = password ?? PasswordGenerator.Generate()
        };
    }
}
