using Bogus;
using PolicyDesk.Communication.Requests;

namespace CommonTestUtilities.Requests;

public static class RequestPolicyholdersJsonBuilder
{
    public static RequestPolicyholdersJson Build(string? cpf = null, string? email = null) =>
        new Faker<RequestPolicyholdersJson>()
            .RuleFor(r => r.Name, f => f.Name.FullName())
            .RuleFor(r => r.Cpf, _ => cpf ?? CpfGenerator.Generate())
            .RuleFor(r => r.BirthDate, f => BirthDateGenerator.Generate(f))
            .RuleFor(r => r.Email, f => email ?? f.Internet.Email())
            .RuleFor(r => r.PhoneNumber, f => f.Phone.PhoneNumber("(##) 9 ####-####"))
            .Generate();
}

public static class RequestPolicyholdersUpdateJsonBuilder
{
    public static RequestPolicyholdersUpdateJson Build(string? email = null) =>
        new Faker<RequestPolicyholdersUpdateJson>()
            .RuleFor(r => r.Name, f => f.Name.FullName())
            .RuleFor(r => r.BirthDate, f => BirthDateGenerator.Generate(f))
            .RuleFor(r => r.Email, f => email ?? f.Internet.Email())
            .RuleFor(r => r.PhoneNumber, f => f.Phone.PhoneNumber("(##) 9 ####-####"))
            .Generate();
}
