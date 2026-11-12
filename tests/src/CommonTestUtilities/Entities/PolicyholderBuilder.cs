using Bogus;
using CommonTestUtilities.Generators;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Validations;

namespace CommonTestUtilities.Entities;

public static class PolicyholderBuilder
{
    public static Policyholder Build(
        string? name = null,
        string? cpf = null,
        DateOnly? birthDate = null,
        string? email = null,
        string? phone = null) =>
        new Faker<Policyholder>()
            .CustomInstantiator(_ => (Policyholder)Activator.CreateInstance(typeof(Policyholder), nonPublic: true)!)
            .RuleFor(policyholder => policyholder.Name, f => name ?? f.Person.FullName)
            .RuleFor(policyholder => policyholder.Cpf, _ => cpf ?? CpfGenerator.Generate())
            .RuleFor(policyholder => policyholder.BirthDate, f => birthDate ?? BirthDateGenerator.Generate(f))
            .RuleFor(policyholder => policyholder.Email, f => email ?? f.Internet.Email(f.Person.FirstName, f.Person.LastName).ToLowerInvariant())
            .RuleFor(policyholder => policyholder.Phone, f => phone ?? PhoneValidator.NormalizeOrThrow(PhoneGenerator.Generate(f)))
            .Generate();

    public static IList<Policyholder> Collection(int count = 2) =>
        Enumerable.Range(0, count).Select(_ => Build()).ToList();
}
