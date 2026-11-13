using CommonTestUtilities.Entities;
using CommonTestUtilities.Generators;
using CommonTestUtilities.Loggers;
using CommonTestUtilities.Repositories.Users;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Security;
using PolicyDesk.Application.Users.Login;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Entities;
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
        var user = UserBuilder.Build();
        var password = PasswordGenerator.Generate();
        var useCase = CreateUseCase(user, password);

        var result = await useCase.ExecuteAsync(RequestLoginJsonBuilder.Build(user.Email, password));

        result.Nome.ShouldBe(user.Name);
        result.Token.ShouldBe(AccessTokenGeneratorBuilder.TokenFor(user.Id));
        result.ExpiraEm.ShouldBe(AccessTokenGeneratorBuilder.ExpiresAt);
    }

    [Fact(DisplayName = nameof(Deve_RetornarToken_Quando_EmailTiverMaiusculasOuEspacos))]
    [Trait("Application", "Users - Login")]
    public async Task Deve_RetornarToken_Quando_EmailTiverMaiusculasOuEspacos()
    {
        var user = UserBuilder.Build();
        var password = PasswordGenerator.Generate();
        var useCase = CreateUseCase(user, password);

        var result = await useCase.ExecuteAsync(RequestLoginJsonBuilder.Build($"  {user.Email.ToUpperInvariant()} ", password));

        result.Token.ShouldBe(AccessTokenGeneratorBuilder.TokenFor(user.Id));
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoAutorizado_Quando_SenhaForIncorreta))]
    [Trait("Application", "Users - Login")]
    public async Task Deve_LancarNaoAutorizado_Quando_SenhaForIncorreta()
    {
        var user = UserBuilder.Build();

        await ShouldFailAsUnauthorized(RequestLoginJsonBuilder.Build(user.Email), user);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoAutorizado_Quando_UsuarioNaoExistirOuEstiverInativo))]
    [Trait("Application", "Users - Login")]
    public async Task Deve_LancarNaoAutorizado_Quando_UsuarioNaoExistirOuEstiverInativo()
    {
        await ShouldFailAsUnauthorized(RequestLoginJsonBuilder.Build());
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacao_Quando_EmailESenhaForemVazios))]
    [Trait("Application", "Users - Login")]
    public async Task Deve_LancarValidacao_Quando_EmailESenhaForemVazios()
    {
        var useCase = CreateUseCase();

        var exception = await useCase.ExecuteAsync(RequestLoginJsonBuilder.Build(email: " ", password: "")).ShouldThrowAsync<ErrorOnValidationException>();
        exception.GetErrors().ShouldSatisfyAllConditions(errorMessage =>
        {
            errorMessage.Count.ShouldBe(2);
            errorMessage.ShouldContain(ResourcesMessageException.INVALID_EMAIL);
            errorMessage.ShouldContain(ResourcesMessageException.USER_PASSWORD_REQUIRED);
        });
    }

    private static async Task ShouldFailAsUnauthorized(RequestLoginJson request, User? user = null)
    {
        var useCase = CreateUseCase(user);

        var exception = await useCase.ExecuteAsync(request).ShouldThrowAsync<UnauthorizedException>();
        exception.Message.ShouldBe(ResourcesMessageException.USER_INVALID_CREDENTIALS);
    }

    private static LoginUseCase CreateUseCase(User? user = null, string? validPassword = null)
    {
        var usersReadOnlyRepositoryBuilder = new UsersReadOnlyRepositoryBuilder();
        var passwordHashingBuilder = new PasswordHashingBuilder();

        if (user is not null)
            usersReadOnlyRepositoryBuilder.GetByEmail(user);

        if (validPassword is not null)
            passwordHashingBuilder.VerifyPassword(validPassword);

        return new LoginUseCase(
            usersReadOnlyRepositoryBuilder.Build(),
            passwordHashingBuilder.Build(),
            AccessTokenGeneratorBuilder.Build(),
            LoggerBuilder.Build<LoginUseCase>());
    }
}
