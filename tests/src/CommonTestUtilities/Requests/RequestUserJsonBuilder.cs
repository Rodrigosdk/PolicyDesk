using Bogus;
using PolicyDesk.Communication.Requests;

namespace CommonTestUtilities.Requests;

public static class RequestUserJsonBuilder
{
    public static RequestUserJson Build(
        string? name = null,
        string? email = null,
        string? password = null) =>
        new Faker<RequestUserJson>()
            .RuleFor(request => request.Nome, f => name ?? f.Person.FullName)
            .RuleFor(request => request.Email, f => email ?? f.Internet.Email(f.Person.FirstName, f.Person.LastName))
            .RuleFor(request => request.Senha, f => password ?? f.Internet.Password(length: 12, prefix: "Aa1!"))
            .Generate();
}
