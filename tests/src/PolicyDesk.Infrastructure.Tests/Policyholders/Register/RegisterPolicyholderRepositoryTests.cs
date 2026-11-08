using CommonTestUtilities.Entities;
using Microsoft.EntityFrameworkCore;
using PolicyDesk.Domain.Repositories.Policyholders;
using Shouldly;

namespace PolicyDesk.Infrastructure.Tests.Policyholders.Register;

public class RegisterPolicyholderRepositoryTests(PostgreSqlInfrastructureFixture fixture)
    : PolicyholdersRepositoryTestBase(fixture)
{
    [Fact(DisplayName = nameof(Deve_InserirSegurado_Quando_DadosForemValidos))]
    [Trait("Infrastructure", "PolicyholdersRepository - Register")]
    public async Task Deve_InserirSegurado_Quando_DadosForemValidos()
    {
        await using var dbContext = await Fixture.CreateCleanDbContextAsync();
        var repository = CreateRepository(dbContext);
        var policyholder = PolicyholderBuilder.Build();

        var result = await repository.RegisterOrReactivateAsync(policyholder);
        var saved = await repository.GetByIdAsync(policyholder.Id);

        result.Result.ShouldBe(PolicyholderRegistrationResult.RegisteredOrReactivated);
        saved.ShouldNotBeNull();
        saved.Cpf.ShouldBe(policyholder.Cpf);
        saved.Email.ShouldBe(policyholder.Email);
        saved.BirthDate.ShouldBe(policyholder.BirthDate);
    }

    [Fact(DisplayName = nameof(Deve_RetornarConflitoDeCpf_Quando_JaExistirSeguradoAtivoComMesmoCpf))]
    [Trait("Infrastructure", "PolicyholdersRepository - Register")]
    public async Task Deve_RetornarConflitoDeCpf_Quando_JaExistirSeguradoAtivoComMesmoCpf()
    {
        await using var dbContext = await Fixture.CreateCleanDbContextAsync();
        var repository = CreateRepository(dbContext);
        var existing = PolicyholderBuilder.Build();
        await repository.RegisterOrReactivateAsync(existing);
        var duplicate = PolicyholderBuilder.Build(cpf: existing.Cpf);

        var result = await repository.RegisterOrReactivateAsync(duplicate);
        var saved = await repository.GetByIdAsync(existing.Id);

        result.Result.ShouldBe(PolicyholderRegistrationResult.ActiveCpfAlreadyExists);
        saved.ShouldNotBeNull();
        saved.Name.ShouldBe(existing.Name);
        saved.Email.ShouldBe(existing.Email);
    }

    [Fact(DisplayName = nameof(Deve_RetornarConflitoDeEmail_Quando_JaExistirSeguradoAtivoComMesmoEmail))]
    [Trait("Infrastructure", "PolicyholdersRepository - Register")]
    public async Task Deve_RetornarConflitoDeEmail_Quando_JaExistirSeguradoAtivoComMesmoEmail()
    {
        await using var dbContext = await Fixture.CreateCleanDbContextAsync();
        var repository = CreateRepository(dbContext);
        var existing = PolicyholderBuilder.Build();
        await repository.RegisterOrReactivateAsync(existing);
        var other = PolicyholderBuilder.Build(email: existing.Email);

        var result = await repository.RegisterOrReactivateAsync(other);

        result.Result.ShouldBe(PolicyholderRegistrationResult.ActiveEmailAlreadyExists);
    }

    [Fact(DisplayName = nameof(Deve_ReativarEAtualizarDados_Quando_CpfEstiverInativo))]
    [Trait("Infrastructure", "PolicyholdersRepository - Register")]
    public async Task Deve_ReativarEAtualizarDados_Quando_CpfEstiverInativo()
    {
        await using var dbContext = await Fixture.CreateCleanDbContextAsync();
        var repository = CreateRepository(dbContext);
        var original = PolicyholderBuilder.Build();
        await repository.RegisterOrReactivateAsync(original);
        await DeactivateAndCommit(dbContext, original);
        var reactivation = PolicyholderBuilder.Build(cpf: original.Cpf);

        var result = await repository.RegisterOrReactivateAsync(reactivation);
        await using var verificationContext = Fixture.CreateDbContext();
        var saved = await verificationContext.Policyholders.IgnoreQueryFilters().SingleAsync(item => item.Id == original.Id);

        result.Result.ShouldBe(PolicyholderRegistrationResult.RegisteredOrReactivated);
        saved.Active.ShouldBeTrue();
        saved.Name.ShouldBe(reactivation.Name);
        saved.Email.ShouldBe(reactivation.Email);
    }
}
