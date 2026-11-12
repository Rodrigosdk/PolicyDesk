using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Security;
using CommonTestUtilities.Time;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolicyDesk.Application.Users.Register;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Users;
using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Users.Register;

public class RegisterUserUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_RegistrarUsuario_Quando_RequisicaoForValida))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_RegistrarUsuario_Quando_RequisicaoForValida()
    {
        var request = RequestUserJsonBuilder.Build(name: "  Jane Doe  ", email: " Jane.Doe@Example.com ");
        var (useCase, writeRepository, unitOfWork) = CreateUseCase();

        var result = await useCase.ExecuteAsync(request);

        result.Id.ShouldNotBe(Guid.Empty);
        writeRepository.Verify(repository => repository.AddAsync(It.Is<User>(user =>
            user.Id == result.Id &&
            user.Name == "Jane Doe" &&
            user.Email == "jane.doe@example.com" &&
            user.Active)), Times.Once);
        unitOfWork.Verify(work => work.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = nameof(Deve_GuardarHashDaSenha_Quando_RegistrarUsuario))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_GuardarHashDaSenha_Quando_RegistrarUsuario()
    {
        var request = RequestUserJsonBuilder.Build();
        var (useCase, writeRepository, _) = CreateUseCase();
        User? savedUser = null;
        writeRepository
            .Setup(repository => repository.AddAsync(It.IsAny<User>()))
            .Callback<User>(user => savedUser = user)
            .Returns(Task.CompletedTask);

        await useCase.ExecuteAsync(request);

        savedUser.ShouldNotBeNull();
        savedUser.Password.ShouldNotBe(request.Senha);
        PasswordHashingBuilder.Build().VerifyPassword(request.Senha, savedUser.Password).ShouldBeTrue();
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoSemSalvar_Quando_JaExistirUsuarioAtivoComMesmoEmail))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_LancarValidacaoSemSalvar_Quando_JaExistirUsuarioAtivoComMesmoEmail()
    {
        var (useCase, writeRepository, unitOfWork) = CreateUseCase(emailInUse: true);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.ExecuteAsync(RequestUserJsonBuilder.Build()));

        exception.GetErrors().ShouldBe([ResourcesMessageException.USER_ACTIVE_EMAIL_EXISTS]);
        VerifyNothingSaved(writeRepository, unitOfWork);
    }

    [Fact(DisplayName = nameof(Deve_ReativarUsuario_Quando_InativadoHaMenosDe30DiasESenhaConferir))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_ReativarUsuario_Quando_InativadoHaMenosDe30DiasESenhaConferir()
    {
        var (inactiveUser, password) = CreateInactiveUser();
        var request = RequestUserJsonBuilder.Build(email: inactiveUser.Email, password: password);
        var (useCase, writeRepository, unitOfWork) = CreateUseCase(inactiveUser: inactiveUser, clock: ClockAfter(days: 29));

        var result = await useCase.ExecuteAsync(request);

        result.Id.ShouldBe(inactiveUser.Id);
        inactiveUser.Active.ShouldBeTrue();
        inactiveUser.DeactivatedAt.ShouldBeNull();
        writeRepository.Verify(repository => repository.AddAsync(It.IsAny<User>()), Times.Never);
        unitOfWork.Verify(work => work.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoSemReativar_Quando_InativadoHaMenosDe30DiasESenhaNaoConferir))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_LancarValidacaoSemReativar_Quando_InativadoHaMenosDe30DiasESenhaNaoConferir()
    {
        var (inactiveUser, _) = CreateInactiveUser();
        var request = RequestUserJsonBuilder.Build(email: inactiveUser.Email);
        var (useCase, writeRepository, unitOfWork) = CreateUseCase(inactiveUser: inactiveUser, clock: ClockAfter(days: 29));

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(() => useCase.ExecuteAsync(request));

        exception.GetErrors().ShouldBe([ResourcesMessageException.USER_INVALID_CREDENTIALS]);
        inactiveUser.Active.ShouldBeFalse();
        VerifyNothingSaved(writeRepository, unitOfWork);
    }

    [Fact(DisplayName = nameof(Deve_RegistrarNovoUsuario_Quando_InativadoHaMaisDe30Dias))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_RegistrarNovoUsuario_Quando_InativadoHaMaisDe30Dias()
    {
        var (inactiveUser, password) = CreateInactiveUser();
        var request = RequestUserJsonBuilder.Build(email: inactiveUser.Email, password: password);
        var (useCase, writeRepository, unitOfWork) = CreateUseCase(inactiveUser: inactiveUser, clock: ClockAfter(days: 31));

        var result = await useCase.ExecuteAsync(request);

        result.Id.ShouldNotBe(inactiveUser.Id);
        inactiveUser.Active.ShouldBeFalse();
        writeRepository.Verify(repository => repository.AddAsync(It.Is<User>(user =>
            user.Id == result.Id && user.Email == inactiveUser.Email)), Times.Once);
        unitOfWork.Verify(work => work.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory(DisplayName = nameof(Deve_LancarValidacaoSemSalvar_Quando_NomeForVazio))]
    [Trait("Application", "Users - Register")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Deve_LancarValidacaoSemSalvar_Quando_NomeForVazio(string name)
    {
        await ShouldFailWithoutSaving(
            RequestUserJsonBuilder.Build(name: name),
            [ResourcesMessageException.USER_NAME_REQUIRED]);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoSemSalvar_Quando_NomeExcederOLimite))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_LancarValidacaoSemSalvar_Quando_NomeExcederOLimite()
    {
        await ShouldFailWithoutSaving(
            RequestUserJsonBuilder.Build(name: new string('a', FieldLimits.UserName + 1)),
            [FieldLimits.UserNameTooLong]);
    }

    [Theory(DisplayName = nameof(Deve_LancarValidacaoSemSalvar_Quando_EmailForInvalido))]
    [Trait("Application", "Users - Register")]
    [InlineData("")]
    [InlineData("john")]
    [InlineData("john@example")]
    public async Task Deve_LancarValidacaoSemSalvar_Quando_EmailForInvalido(string email)
    {
        await ShouldFailWithoutSaving(
            RequestUserJsonBuilder.Build(email: email),
            [ResourcesMessageException.INVALID_EMAIL]);
    }

    [Theory(DisplayName = nameof(Deve_LancarValidacaoSemSalvar_Quando_SenhaForVazia))]
    [Trait("Application", "Users - Register")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Deve_LancarValidacaoSemSalvar_Quando_SenhaForVazia(string password)
    {
        await ShouldFailWithoutSaving(
            RequestUserJsonBuilder.Build(password: password),
            [ResourcesMessageException.USER_PASSWORD_REQUIRED]);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoSemSalvar_Quando_SenhaForMenorQueOMinimo))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_LancarValidacaoSemSalvar_Quando_SenhaForMenorQueOMinimo()
    {
        await ShouldFailWithoutSaving(
            RequestUserJsonBuilder.Build(password: new string('a', FieldLimits.UserPasswordMinLength - 1)),
            [FieldLimits.UserPasswordTooShort]);
    }

    [Fact(DisplayName = nameof(Deve_RetornarTodosOsErros_Quando_VariosCamposForemInvalidos))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_RetornarTodosOsErros_Quando_VariosCamposForemInvalidos()
    {
        await ShouldFailWithoutSaving(
            RequestUserJsonBuilder.Build(name: "", email: "invalido", password: ""),
            [
                ResourcesMessageException.USER_NAME_REQUIRED,
                ResourcesMessageException.INVALID_EMAIL,
                ResourcesMessageException.USER_PASSWORD_REQUIRED
            ]);
    }

    private static async Task ShouldFailWithoutSaving(RequestUserJson request, string[] expectedErrors)
    {
        var (useCase, writeRepository, unitOfWork) = CreateUseCase();

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(() => useCase.ExecuteAsync(request));

        exception.GetErrors().ShouldBe(expectedErrors);
        VerifyNothingSaved(writeRepository, unitOfWork);
    }

    private static void VerifyNothingSaved(Mock<IUsersWriteOnlyRepository> writeRepository, Mock<IUnitOfWork> unitOfWork)
    {
        writeRepository.Verify(repository => repository.AddAsync(It.IsAny<User>()), Times.Never);
        unitOfWork.Verify(work => work.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static (User User, string Password) CreateInactiveUser()
    {
        var password = PasswordGenerator.Generate();
        var user = UserBuilder.Build(password: PasswordHashingBuilder.Build().HashPassword(password));
        user.Deactivate();
        return (user, password);
    }

    private static BusinessClock ClockAfter(int days) =>
        BusinessClockBuilder.Build(DateTimeOffset.UtcNow.AddDays(days));

    private static (
        RegisterUserUseCase UseCase,
        Mock<IUsersWriteOnlyRepository> WriteRepository,
        Mock<IUnitOfWork> UnitOfWork)
        CreateUseCase(bool emailInUse = false, User? inactiveUser = null, BusinessClock? clock = null)
    {
        var readRepository = new UsersReadOnlyRepositoryBuilder();
        if (emailInUse)
            readRepository.ExistActiveWithEmail();
        var updateRepository = new UsersUpdateOnlyRepositoryBuilder();
        if (inactiveUser is not null)
            updateRepository.GetLatestInactiveByEmailForUpdate(inactiveUser);
        var writeRepository = UsersWriteOnlyRepositoryBuilder.Build();
        var unitOfWork = UnitOfWorkBuilder.Build();
        return (
            new RegisterUserUseCase(
                writeRepository.Object,
                readRepository.Build(),
                updateRepository.Build(),
                PasswordHashingBuilder.Build(),
                unitOfWork.Object,
                clock ?? BusinessClockBuilder.Build(),
                NullLogger<RegisterUserUseCase>.Instance),
            writeRepository,
            unitOfWork);
    }
}
