using Bogus;
using PolicyDesk.Communication.Requests;

namespace CommonTestUtilities.Requests;

public static class RequestPolicyholderJsonBuilder
{
    public static RequestPolicyholderJson Build(string? cpf = null, string? email = null) =>
        new Faker<RequestPolicyholderJson>()
            .RuleFor(r => r.Nome, f => f.Name.FullName())
            .RuleFor(r => r.Cpf, _ => cpf ?? CpfGenerator.Generate())
            .RuleFor(r => r.DataNascimento, f => BirthDateGenerator.Generate(f))
            .RuleFor(r => r.Email, f => email ?? f.Internet.Email())
            .RuleFor(r => r.Telefone, f => PhoneGenerator.Generate(f))
            .Generate();
}

public static class RequestUpdatePolicyholderJsonBuilder
{
    public static RequestUpdatePolicyholderJson Build(string? email = null) =>
        new Faker<RequestUpdatePolicyholderJson>()
            .RuleFor(r => r.Nome, f => f.Name.FullName())
            .RuleFor(r => r.DataNascimento, f => BirthDateGenerator.Generate(f))
            .RuleFor(r => r.Email, f => email ?? f.Internet.Email())
            .RuleFor(r => r.Telefone, f => PhoneGenerator.Generate(f))
            .Generate();
}
