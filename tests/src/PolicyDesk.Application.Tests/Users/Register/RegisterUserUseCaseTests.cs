using CommonTestUtilities.Entities;
using CommonTestUtilities.Generators;
using CommonTestUtilities.Loggers;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Repositories.Users;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Security;
using CommonTestUtilities.Time;
using PolicyDesk.Application.Users.Register;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;
using Xunit.Sdk;

namespace PolicyDesk.Application.Tests.Users.Register;

public class RegisterUserUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_RegistrarUsuario_Quando_RequisicaoForValida))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_RegistrarUsuario_Quando_RequisicaoForValida()
    {
        var request = RequestUserJsonBuilder.Build();
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(request);

        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacao_Quando_JaExistirUsuarioAtivoComMesmoEmail))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_LancarValidacao_Quando_JaExistirUsuarioAtivoComMesmoEmail()
    {
        var request = RequestUserJsonBuilder.Build();
        var useCase = CreateUseCase(emailInUse: true);

        var exception = await useCase.ExecuteAsync(request).ShouldThrowAsync<ErrorOnValidationException>();
        exception.GetErrors().ShouldSatisfyAllConditions(errorMessage =>
        {
            errorMessage.Count.ShouldBe(1);
            errorMessage.ShouldContain(ResourcesMessageException.USER_ACTIVE_EMAIL_EXISTS);
        });
    }

    [Fact(DisplayName = nameof(Deve_ReativarUsuario_Quando_InativadoHaMenosDe30DiasESenhaConferir))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_ReativarUsuario_Quando_InativadoHaMenosDe30DiasESenhaConferir()
    {
        var (inactiveUser, password) = CreateInactiveUser();
        var request = RequestUserJsonBuilder.Build(email: inactiveUser.Email, password: password);
        var useCase = CreateUseCase(inactiveUser: inactiveUser, validPassword: password, clock: ClockAfter(days: 29));

        var result = await useCase.ExecuteAsync(request);

        result.Id.ShouldBe(inactiveUser.Id);
        inactiveUser.Active.ShouldBeTrue();
        inactiveUser.DeactivatedAt.ShouldBeNull();
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoSemReativar_Quando_InativadoHaMenosDe30DiasESenhaNaoConferir))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_LancarValidacaoSemReativar_Quando_InativadoHaMenosDe30DiasESenhaNaoConferir()
    {
        var (inactiveUser, _) = CreateInactiveUser();
        var request = RequestUserJsonBuilder.Build(email: inactiveUser.Email);
        var useCase = CreateUseCase(inactiveUser: inactiveUser, clock: ClockAfter(days: 29));

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(() => useCase.ExecuteAsync(request));

        exception.GetErrors().ShouldBe([ResourcesMessageException.USER_INVALID_CREDENTIALS]);
        inactiveUser.Active.ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_RegistrarNovoUsuario_Quando_InativadoHaMaisDe30Dias))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_RegistrarNovoUsuario_Quando_InativadoHaMaisDe30Dias()
    {
        var (inactiveUser, password) = CreateInactiveUser();
        var request = RequestUserJsonBuilder.Build(email: inactiveUser.Email, password: password);
        var useCase = CreateUseCase(inactiveUser: inactiveUser, validPassword: password, clock: ClockAfter(days: 31));

        var result = await useCase.ExecuteAsync(request);

        result.Id.ShouldNotBe(inactiveUser.Id);
        inactiveUser.Active.ShouldBeFalse();
    }

    [Theory(DisplayName = nameof(Deve_LancarValidacao_Quando_NomeForVazio))]
    [Trait("Application", "Users - Register")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Deve_LancarValidacao_Quando_NomeForVazio(string name)
    {
        await ShouldFailWithErrors(
            RequestUserJsonBuilder.Build(name: name),
            [ResourcesMessageException.USER_NAME_REQUIRED]);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacao_Quando_NomeExcederOLimite))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_LancarValidacao_Quando_NomeExcederOLimite()
    {
        await ShouldFailWithErrors(
            RequestUserJsonBuilder.Build(name: new string('a', FieldLimits.UserName + 1)),
            [FieldLimits.UserNameTooLong]);
    }

    [Theory(DisplayName = nameof(Deve_LancarValidacao_Quando_EmailForInvalido))]
    [Trait("Application", "Users - Register")]
    [InlineData("")]
    [InlineData("john")]
    [InlineData("john@example")]
    public async Task Deve_LancarValidacao_Quando_EmailForInvalido(string email)
    {
        await ShouldFailWithErrors(
            RequestUserJsonBuilder.Build(email: email),
            [ResourcesMessageException.INVALID_EMAIL]);
    }

    [Theory(DisplayName = nameof(Deve_LancarValidacao_Quando_SenhaForVazia))]
    [Trait("Application", "Users - Register")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Deve_LancarValidacao_Quando_SenhaForVazia(string password)
    {
        await ShouldFailWithErrors(
            RequestUserJsonBuilder.Build(password: password),
            [ResourcesMessageException.USER_PASSWORD_REQUIRED]);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacao_Quando_SenhaForMenorQueOMinimo))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_LancarValidacao_Quando_SenhaForMenorQueOMinimo()
    {
        await ShouldFailWithErrors(
            RequestUserJsonBuilder.Build(password: new string('a', FieldLimits.UserPasswordMinLength - 1)),
            [FieldLimits.UserPasswordTooShort]);
    }

    [Fact(DisplayName = nameof(Deve_RetornarTodosOsErros_Quando_VariosCamposForemInvalidos))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_RetornarTodosOsErros_Quando_VariosCamposForemInvalidos()
    {
        await ShouldFailWithErrors(
            RequestUserJsonBuilder.Build(name: "", email: "invalido", password: ""),
            [
                ResourcesMessageException.USER_NAME_REQUIRED,
                ResourcesMessageException.INVALID_EMAIL,
                ResourcesMessageException.USER_PASSWORD_REQUIRED
            ]);
    }

    private static async Task ShouldFailWithErrors(RequestUserJson request, string[] expectedErrors)
    {
        var useCase = CreateUseCase();

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(() => useCase.ExecuteAsync(request));

        exception.GetErrors().ShouldBe(expectedErrors);
    }

    private static (User User, string Password) CreateInactiveUser()
    {
        var password = PasswordGenerator.Generate();
        var user = UserBuilder.Build();
        user.Deactivate();
        return (user, password);
    }

    private static BusinessClock ClockAfter(int days) =>
        BusinessClockBuilder.Build(DateTimeOffset.UtcNow.AddDays(days));

    private static RegisterUserUseCase CreateUseCase(
        bool emailInUse = false,
        User? inactiveUser = null,
        string? validPassword = null,
        BusinessClock? clock = null)
    {
        var usersReadOnlyRepositoryBuilder = new UsersReadOnlyRepositoryBuilder();
        var usersUpdateOnlyRepositoryBuilder = new UsersUpdateOnlyRepositoryBuilder();
        var passwordHashingBuilder = new PasswordHashingBuilder();

        if (emailInUse)
            usersReadOnlyRepositoryBuilder.ExistActiveWithEmail();

        if (inactiveUser is not null)
            usersUpdateOnlyRepositoryBuilder.GetLatestInactiveByEmailForUpdate(inactiveUser);

        if (validPassword is not null)
            passwordHashingBuilder.VerifyPassword(validPassword);

        return new RegisterUserUseCase(
            UsersWriteOnlyRepositoryBuilder.Build(),
            usersReadOnlyRepositoryBuilder.Build(),
            usersUpdateOnlyRepositoryBuilder.Build(),
            passwordHashingBuilder.Build(),
            UnitOfWorkBuilder.Build(),
            clock ?? BusinessClockBuilder.Build(),
            LoggerBuilder.Build<RegisterUserUseCase>());
    }
}
