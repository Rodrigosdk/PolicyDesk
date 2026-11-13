using Bogus;
using CommonTestUtilities.Generators;
using PolicyDesk.Communication.Requests;

namespace CommonTestUtilities.Requests;

public static class RequestPolicyholderJsonBuilder
{
    public static RequestPolicyholderJson Build(string? cpf = null, string? email = null) =>
        new Faker<RequestPolicyholderJson>()
            .RuleFor(request => request.Nome, f => f.Name.FullName())
            .RuleFor(request => request.Cpf, _ => cpf ?? CpfGenerator.Generate())
            .RuleFor(request => request.DataNascimento, f => BirthDateGenerator.Generate(f))
            .RuleFor(request => request.Email, f => email ?? f.Internet.Email())
            .RuleFor(request => request.Telefone, f => PhoneGenerator.Generate(f))
            .Generate();
}

public static class RequestUpdatePolicyholderJsonBuilder
{
    public static RequestUpdatePolicyholderJson Build(string? email = null) =>
        new Faker<RequestUpdatePolicyholderJson>()
            .RuleFor(request => request.Nome, f => f.Name.FullName())
            .RuleFor(request => request.DataNascimento, f => BirthDateGenerator.Generate(f))
            .RuleFor(request => request.Email, f => email ?? f.Internet.Email())
            .RuleFor(request => request.Telefone, f => PhoneGenerator.Generate(f))
            .Generate();
}
