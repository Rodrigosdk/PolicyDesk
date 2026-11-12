using Bogus;
using PolicyDesk.Communication.Requests;

namespace CommonTestUtilities.Requests;

public static class RequestLoginJsonBuilder
{
    public static RequestLoginJson Build(
        string? email = null,
        string? password = null) =>
        new Faker<RequestLoginJson>()
            .RuleFor(request => request.Email, f => email ?? f.Internet.Email())
            .RuleFor(request => request.Senha, f => password ?? f.Internet.Password(length: 12, prefix: "Aa1!"))
            .Generate();
}
