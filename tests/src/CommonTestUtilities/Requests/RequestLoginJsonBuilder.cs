using Bogus;
using PolicyDesk.Communication.Requests;

namespace CommonTestUtilities.Requests;

public static class RequestLoginJsonBuilder
{
    public static RequestLoginJson Build(string? email = null, string? password = null) => new()
    {
        Email = email ?? new Faker().Internet.Email(),
        Senha = password ?? PasswordGenerator.Generate()
    };
}
