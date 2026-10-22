using Bogus;
using CommonTestUtilities.Requests;
using Policyholder = PolicyDesk.Domain.Entities.Policyholders;

namespace CommonTestUtilities.Entities;

public static class PolicyholderBuilder
{
    public static Policyholder Build(
        string? name = null,
        string? cpf = null,
        DateTime? birthDate = null,
        string? email = null,
        string? phone = null)
    {
        var faker = new Faker();
        return new Policyholder(
            name ?? faker.Name.FullName(),
            cpf ?? CpfGenerator.Generate(),
            birthDate ?? BirthDateGenerator.Generate(faker),
            email ?? faker.Internet.Email(),
            phone ?? faker.Phone.PhoneNumber("(##) 9 ####-####"));
    }

    public static IList<Policyholder> Collection(int count = 2) =>
        Enumerable.Range(0, count).Select(_ => Build()).ToList();
}
