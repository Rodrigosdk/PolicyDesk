using CommonTestUtilities.Entities;
using CommonTestUtilities.Generators;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Security;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolicyDesk.Application.Users.Login;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Security.Tokens;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Users.Login;

public class LoginUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_RetornarToken_Quando_CredenciaisForemValidas))]
    [Trait("Application", "Users - Login")]
    public async Task Deve_RetornarToken_Quando_CredenciaisForemValidas()
    {
        var (user, password) = CreateUser();
        var (useCase, tokenGenerator) = CreateUseCase(user);

        var result = await useCase.ExecuteAsync(RequestLoginJsonBuilder.Build(user.Email, password));

        result.Nome.ShouldBe(user.Name);
        result.Token.ShouldBe(AccessTokenGeneratorBuilder.TokenFor(user.Id));
        result.ExpiraEm.ShouldBe(AccessTokenGeneratorBuilder.ExpiresAt);
        tokenGenerator.Verify(generator => generator.Generate(user.Id), Times.Once);
    }

    [Fact(DisplayName = nameof(Deve_RetornarToken_Quando_EmailTiverMaiusculasOuEspacos))]
    [Trait("Application", "Users - Login")]
    public async Task Deve_RetornarToken_Quando_EmailTiverMaiusculasOuEspacos()
    {
        var (user, password) = CreateUser();
        var (useCase, _) = CreateUseCase(user);

        var result = await useCase.ExecuteAsync(RequestLoginJsonBuilder.Build($"  {user.Email.ToUpperInvariant()} ", password));

        result.Token.ShouldBe(AccessTokenGeneratorBuilder.TokenFor(user.Id));
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoAutorizadoSemGerarToken_Quando_SenhaForIncorreta))]
    [Trait("Application", "Users - Login")]
    public async Task Deve_LancarNaoAutorizadoSemGerarToken_Quando_SenhaForIncorreta()
    {
        var (user, _) = CreateUser();

        await ShouldFailAsUnauthorized(RequestLoginJsonBuilder.Build(user.Email), user);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoAutorizadoSemGerarToken_Quando_UsuarioNaoExistirOuEstiverInativo))]
    [Trait("Application", "Users - Login")]
    public async Task Deve_LancarNaoAutorizadoSemGerarToken_Quando_UsuarioNaoExistirOuEstiverInativo()
    {
        await ShouldFailAsUnauthorized(RequestLoginJsonBuilder.Build());
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoSemGerarToken_Quando_EmailESenhaForemVazios))]
    [Trait("Application", "Users - Login")]
    public async Task Deve_LancarValidacaoSemGerarToken_Quando_EmailESenhaForemVazios()
    {
        var (useCase, tokenGenerator) = CreateUseCase();

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.ExecuteAsync(RequestLoginJsonBuilder.Build(email: " ", password: "")));

        exception.GetErrors().ShouldBe([ResourcesMessageException.INVALID_EMAIL, ResourcesMessageException.USER_PASSWORD_REQUIRED]);
        tokenGenerator.Verify(generator => generator.Generate(It.IsAny<Guid>()), Times.Never);
    }

    private static async Task ShouldFailAsUnauthorized(RequestLoginJson request, User? user = null)
    {
        var (useCase, tokenGenerator) = CreateUseCase(user);

        var exception = await Should.ThrowAsync<UnauthorizedException>(() => useCase.ExecuteAsync(request));

        exception.Message.ShouldBe(ResourcesMessageException.USER_INVALID_CREDENTIALS);
        tokenGenerator.Verify(generator => generator.Generate(It.IsAny<Guid>()), Times.Never);
    }

    private static (User User, string Password) CreateUser()
    {
        var password = PasswordGenerator.Generate();
        return (UserBuilder.Build(password: PasswordHashingBuilder.Build().HashPassword(password)), password);
    }

    private static (LoginUseCase UseCase, Mock<IAccessTokenGenerator> TokenGenerator) CreateUseCase(User? user = null)
    {
        var readRepository = new UsersReadOnlyRepositoryBuilder();
        if (user is not null)
            readRepository.GetByEmail(user);
        var tokenGenerator = AccessTokenGeneratorBuilder.Build();
        return (
            new LoginUseCase(readRepository.Build(), PasswordHashingBuilder.Build(), tokenGenerator.Object, NullLogger<LoginUseCase>.Instance),
            tokenGenerator);
    }
}
