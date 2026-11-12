using CommonTestUtilities.Entities;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Domain.Tests.Entities;

public class UsersTests
{
    [Fact(DisplayName = nameof(Deve_InstanciarUsuarioAtivo_Quando_DadosForemValidos))]
    [Trait("Domain", "Users - Instancia")]
    public void Deve_InstanciarUsuarioAtivo_Quando_DadosForemValidos()
    {
        var beforeCreation = DateTime.UtcNow;

        var user = UserBuilder.Build(
            name: "  John Doe  ",
            email: "  John.Doe@Example.com ",
            password: "hash-da-senha");

        var afterCreation = DateTime.UtcNow;
        user.Id.ShouldNotBe(Guid.Empty);
        user.Name.ShouldBe("John Doe");
        user.Email.ShouldBe("john.doe@example.com");
        user.Password.ShouldBe("hash-da-senha");
        user.Active.ShouldBeTrue();
        user.CreatedAt.ShouldBeInRange(beforeCreation, afterCreation);
        user.UpdatedAt.ShouldBeNull();
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_NomeForVazio))]
    [Trait("Domain", "Users - Nome")]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_LancarExcecao_Quando_NomeForVazio(string name)
    {
        var exception = Should.Throw<DomainRuleException>(() => UserBuilder.Build(name: name));

        exception.Message.ShouldBe(ResourcesMessageException.USER_NAME_REQUIRED);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecao_Quando_NomeExcederOLimite))]
    [Trait("Domain", "Users - Nome")]
    public void Deve_LancarExcecao_Quando_NomeExcederOLimite()
    {
        var name = new string('a', FieldLimits.UserName + 1);

        var exception = Should.Throw<DomainRuleException>(() => UserBuilder.Build(name: name));

        exception.Message.ShouldBe(FieldLimits.UserNameTooLong);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_EmailForInvalido))]
    [Trait("Domain", "Users - Email")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("john")]
    [InlineData("john@example")]
    [InlineData("john@@example.com")]
    public void Deve_LancarExcecao_Quando_EmailForInvalido(string email)
    {
        var exception = Should.Throw<DomainRuleException>(() => UserBuilder.Build(email: email));

        exception.Message.ShouldBe(ResourcesMessageException.INVALID_EMAIL);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_SenhaForVazia))]
    [Trait("Domain", "Users - Senha")]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_LancarExcecao_Quando_SenhaForVazia(string password)
    {
        var exception = Should.Throw<DomainRuleException>(() => UserBuilder.Build(password: password));

        exception.Message.ShouldBe(ResourcesMessageException.USER_PASSWORD_REQUIRED);
    }

    [Fact(DisplayName = nameof(Deve_AtualizarOsDados_Quando_DadosForemValidos))]
    [Trait("Domain", "Users - Atualização")]
    public void Deve_AtualizarOsDados_Quando_DadosForemValidos()
    {
        var user = UserBuilder.Build();
        var beforeUpdate = DateTime.UtcNow;

        user.Update("  Jane Doe  ", " Jane.Doe@Example.com ", "novo-hash-da-senha");

        user.Name.ShouldBe("Jane Doe");
        user.Email.ShouldBe("jane.doe@example.com");
        user.Password.ShouldBe("novo-hash-da-senha");
        user.UpdatedAt.ShouldNotBeNull().ShouldBeInRange(beforeUpdate, DateTime.UtcNow);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecaoSemAlterar_Quando_AtualizarComDadosInvalidos))]
    [Trait("Domain", "Users - Atualização")]
    [InlineData("", "jane.doe@example.com", "novo-hash-da-senha")]
    [InlineData("Jane Doe", "invalido", "novo-hash-da-senha")]
    [InlineData("Jane Doe", "jane.doe@example.com", "")]
    public void Deve_LancarExcecaoSemAlterar_Quando_AtualizarComDadosInvalidos(string name, string email, string password)
    {
        var user = UserBuilder.Build();
        var originalName = user.Name;
        var originalEmail = user.Email;
        var originalPassword = user.Password;

        Should.Throw<DomainRuleException>(() => user.Update(name, email, password));

        user.Name.ShouldBe(originalName);
        user.Email.ShouldBe(originalEmail);
        user.Password.ShouldBe(originalPassword);
        user.UpdatedAt.ShouldBeNull();
    }

    [Fact(DisplayName = nameof(Deve_Inativar_Quando_UsuarioEstiverAtivo))]
    [Trait("Domain", "Users - Inativação")]
    public void Deve_Inativar_Quando_UsuarioEstiverAtivo()
    {
        var user = UserBuilder.Build();

        user.Deactivate();

        user.Active.ShouldBeFalse();
        user.UpdatedAt.ShouldNotBeNull();
    }

    [Fact(DisplayName = nameof(Deve_RegistrarDataDeInativacao_Quando_Inativar))]
    [Trait("Domain", "Users - Inativação")]
    public void Deve_RegistrarDataDeInativacao_Quando_Inativar()
    {
        var user = UserBuilder.Build();
        var beforeDeactivation = DateTime.UtcNow;

        user.Deactivate();

        user.DeactivatedAt.ShouldNotBeNull().ShouldBeInRange(beforeDeactivation, DateTime.UtcNow);
    }

    [Theory(DisplayName = nameof(Deve_PermitirReativacao_Quando_InativadoDentroDoPrazo))]
    [Trait("Domain", "Users - Reativação")]
    [InlineData(0)]
    [InlineData(29)]
    public void Deve_PermitirReativacao_Quando_InativadoDentroDoPrazo(int daysAfterDeactivation)
    {
        var user = UserBuilder.Build();
        user.Deactivate();

        var deactivatedAt = user.DeactivatedAt.ShouldNotBeNull();

        user.CanBeReactivated(deactivatedAt.AddDays(daysAfterDeactivation)).ShouldBeTrue();
        user.CanBeReactivated(deactivatedAt.Add(User.ReactivationPeriod)).ShouldBeTrue();
    }

    [Fact(DisplayName = nameof(Deve_RecusarReativacao_Quando_PrazoDeReativacaoTiverPassado))]
    [Trait("Domain", "Users - Reativação")]
    public void Deve_RecusarReativacao_Quando_PrazoDeReativacaoTiverPassado()
    {
        var user = UserBuilder.Build();
        user.Deactivate();

        var deactivatedAt = user.DeactivatedAt.ShouldNotBeNull();

        user.CanBeReactivated(deactivatedAt.Add(User.ReactivationPeriod).AddTicks(1)).ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_RecusarReativacao_Quando_UsuarioEstiverAtivo))]
    [Trait("Domain", "Users - Reativação")]
    public void Deve_RecusarReativacao_Quando_UsuarioEstiverAtivo()
    {
        var user = UserBuilder.Build();

        user.CanBeReactivated(DateTime.UtcNow).ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_Reativar_Quando_UsuarioEstiverInativo))]
    [Trait("Domain", "Users - Reativação")]
    public void Deve_Reativar_Quando_UsuarioEstiverInativo()
    {
        var user = UserBuilder.Build();
        user.Deactivate();

        user.Reactivate();

        user.Active.ShouldBeTrue();
        user.DeactivatedAt.ShouldBeNull();
        user.UpdatedAt.ShouldNotBeNull();
    }
}
