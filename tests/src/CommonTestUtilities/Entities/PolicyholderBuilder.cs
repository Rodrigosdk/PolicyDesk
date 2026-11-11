using Bogus;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Time;
using PolicyDesk.Domain.Entities;

namespace CommonTestUtilities.Entities;

public static class PolicyholderBuilder
{
    public static Policyholder Build(
        string? name = null,
        string? cpf = null,
        DateOnly? birthDate = null,
        string? email = null,
        string? phone = null,
        DateOnly? today = null)
    {
        var faker = new Faker();
        return new Policyholder(
            name ?? faker.Name.FullName(),
            cpf ?? CpfGenerator.Generate(),
            birthDate ?? BirthDateGenerator.Generate(faker),
            email ?? faker.Internet.Email(),
            phone ?? PhoneGenerator.Generate(faker),
            today ?? BusinessClockBuilder.DefaultToday);
    }

    public static IList<Policyholder> Collection(int count = 2) =>
        Enumerable.Range(0, count).Select(_ => Build()).ToList();
}
