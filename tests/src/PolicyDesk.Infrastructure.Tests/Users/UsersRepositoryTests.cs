using CommonTestUtilities.Entities;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using PolicyDesk.Infrastructure.DataAccess;
using PolicyDesk.Infrastructure.Tests.Policyholders;
using Shouldly;

namespace PolicyDesk.Infrastructure.Tests.Users;

public class UsersRepositoryTests(PostgreSqlInfrastructureFixture fixture) : UsersRepositoryTestBase(fixture)
{
    private static async Task<User> SaveUser(PolicyDeskDbContext dbContext, User? user = null)
    {
        user ??= UserBuilder.Build();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    private static async Task<User> SaveInactiveUser(PolicyDeskDbContext dbContext)
    {
        var user = UserBuilder.Build();
        user.Deactivate();
        return await SaveUser(dbContext, user);
    }

    [Fact(DisplayName = nameof(Deve_AdicionarEBuscarUsuario_Quando_UsuarioForValido))]
    [Trait("Infrastructure", "UsersRepository - Cadastro e Consulta")]
    public async Task Deve_AdicionarEBuscarUsuario_Quando_UsuarioForValido()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var user = UserBuilder.Build();

        await CreateRepository(dbContext).AddAsync(user);
        await new UnitOfWork(dbContext).CommitAsync();

        await using var verificationContext = Fixture.CreateDbContext();
        var saved = await CreateRepository(verificationContext).GetByIdAsync(user.Id);
        saved.ShouldNotBeNull();
        saved.Name.ShouldBe(user.Name);
        saved.Email.ShouldBe(user.Email);
        saved.Password.ShouldBe(user.Password);
        saved.Active.ShouldBeTrue();
    }

    [Fact(DisplayName = nameof(Deve_RetornarNulo_Quando_BuscarUsuarioInexistente))]
    [Trait("Infrastructure", "UsersRepository - Consulta")]
    public async Task Deve_RetornarNulo_Quando_BuscarUsuarioInexistente()
    {
        await using var dbContext = Fixture.CreateDbContext();

        (await CreateRepository(dbContext).GetByIdAsync(Guid.NewGuid())).ShouldBeNull();
    }

    [Fact(DisplayName = nameof(Deve_RetornarUsuario_Quando_BuscarPorEmail))]
    [Trait("Infrastructure", "UsersRepository - Consulta")]
    public async Task Deve_RetornarUsuario_Quando_BuscarPorEmail()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var user = await SaveUser(dbContext);

        var result = await CreateRepository(dbContext).GetByEmailAsync(user.Email);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(user.Id);
    }

    [Fact(DisplayName = nameof(Deve_IgnorarUsuarioInativo_Quando_BuscarPorIdOuEmail))]
    [Trait("Infrastructure", "UsersRepository - Consulta")]
    public async Task Deve_IgnorarUsuarioInativo_Quando_BuscarPorIdOuEmail()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var repository = CreateRepository(dbContext);
        var user = await SaveInactiveUser(dbContext);

        (await repository.GetByIdAsync(user.Id)).ShouldBeNull();
        (await repository.GetByEmailAsync(user.Email)).ShouldBeNull();
        (await repository.GetByIdForUpdateAsync(user.Id)).ShouldBeNull();
    }

    [Fact(DisplayName = nameof(Deve_RetornarUsuarioInativo_Quando_BuscarIncluindoInativos))]
    [Trait("Infrastructure", "UsersRepository - Consulta")]
    public async Task Deve_RetornarUsuarioInativo_Quando_BuscarIncluindoInativos()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var user = await SaveInactiveUser(dbContext);

        var result = await CreateRepository(dbContext).GetByIdIncludingInactiveAsync(user.Id);

        result.ShouldNotBeNull();
        result.Active.ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_RetornarUltimoUsuarioInativado_Quando_BuscarInativoPorEmail))]
    [Trait("Infrastructure", "UsersRepository - Consulta")]
    public async Task Deve_RetornarUltimoUsuarioInativado_Quando_BuscarInativoPorEmail()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var older = UserBuilder.Build();
        older.Deactivate();
        await SaveUser(dbContext, older);
        var newer = UserBuilder.Build(email: older.Email);
        newer.Deactivate();
        await SaveUser(dbContext, newer);

        var result = await CreateRepository(dbContext).GetLatestInactiveByEmailForUpdateAsync(older.Email);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(newer.Id);
        result.DeactivatedAt.ShouldNotBeNull();
    }

    [Fact(DisplayName = nameof(Deve_RetornarNulo_Quando_BuscarInativoPorEmailDeUsuarioAtivo))]
    [Trait("Infrastructure", "UsersRepository - Consulta")]
    public async Task Deve_RetornarNulo_Quando_BuscarInativoPorEmailDeUsuarioAtivo()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var user = await SaveUser(dbContext);

        (await CreateRepository(dbContext).GetLatestInactiveByEmailForUpdateAsync(user.Email)).ShouldBeNull();
    }

    [Fact(DisplayName = nameof(Deve_IndicarEmailEmUso_Quando_ExistirUsuarioAtivoComMesmoEmail))]
    [Trait("Infrastructure", "UsersRepository - Email")]
    public async Task Deve_IndicarEmailEmUso_Quando_ExistirUsuarioAtivoComMesmoEmail()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var repository = CreateRepository(dbContext);
        var user = await SaveUser(dbContext);

        (await repository.ExistActiveUserWithEmailAsync(user.Email)).ShouldBeTrue();
        (await repository.ExistActiveUserWithEmailAsync(user.Email, user.Id)).ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_IndicarEmailLivre_Quando_UsuarioComMesmoEmailEstiverInativo))]
    [Trait("Infrastructure", "UsersRepository - Email")]
    public async Task Deve_IndicarEmailLivre_Quando_UsuarioComMesmoEmailEstiverInativo()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var user = await SaveInactiveUser(dbContext);

        (await CreateRepository(dbContext).ExistActiveUserWithEmailAsync(user.Email)).ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_RecusarCadastro_Quando_JaExistirUsuarioAtivoComMesmoEmail))]
    [Trait("Infrastructure", "UsersRepository - Email")]
    public async Task Deve_RecusarCadastro_Quando_JaExistirUsuarioAtivoComMesmoEmail()
    {
        await using var firstRequest = Fixture.CreateDbContext();
        await using var secondRequest = Fixture.CreateDbContext();
        var first = UserBuilder.Build();
        var second = UserBuilder.Build(email: first.Email);
        await CreateRepository(firstRequest).AddAsync(first);
        await CreateRepository(secondRequest).AddAsync(second);

        await new UnitOfWork(firstRequest).CommitAsync();
        var exception = await Should.ThrowAsync<ErrorOnValidationException>(() => new UnitOfWork(secondRequest).CommitAsync());

        exception.GetErrors().ShouldBe([ResourcesMessageException.USER_ACTIVE_EMAIL_EXISTS]);
        await using var verificationContext = Fixture.CreateDbContext();
        (await CreateRepository(verificationContext).GetByIdAsync(first.Id)).ShouldNotBeNull();
        (await CreateRepository(verificationContext).GetByIdAsync(second.Id)).ShouldBeNull();
    }

    [Fact(DisplayName = nameof(Deve_PersistirAlteracoes_Quando_ConfirmarUnidadeDeTrabalho))]
    [Trait("Infrastructure", "UsersRepository - Persistência")]
    public async Task Deve_PersistirAlteracoes_Quando_ConfirmarUnidadeDeTrabalho()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var created = await SaveUser(dbContext);
        var newData = UserBuilder.Build();
        var user = await CreateRepository(dbContext).GetByIdForUpdateAsync(created.Id);
        user.ShouldNotBeNull();

        user.Update(newData.Name, newData.Email, newData.Password);
        await new UnitOfWork(dbContext).CommitAsync();

        await using var verificationContext = Fixture.CreateDbContext();
        var saved = await CreateRepository(verificationContext).GetByIdAsync(created.Id);
        saved.ShouldNotBeNull();
        saved.Name.ShouldBe(newData.Name);
        saved.Email.ShouldBe(newData.Email);
        saved.Password.ShouldBe(newData.Password);
        saved.UpdatedAt.ShouldNotBeNull();
    }
}
