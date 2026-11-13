using CommonTestUtilities.Entities;
using CommonTestUtilities.Loggers;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Repositories.Policyholders;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Time;
using PolicyDesk.Application.Policyholders.Update;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Policyholders.Update;

public class UpdatePolicyholderUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_AtualizarSegurado_Quando_RequisicaoForValida))]
    [Trait("Application", "Policyholders - Update")]
    public async Task Deve_AtualizarSegurado_Quando_RequisicaoForValida()
    {
        var policyholder = PolicyholderBuilder.Build();
        var request = RequestUpdatePolicyholderJsonBuilder.Build(email: "Jane.Updated@Example.com");
        request.Telefone = "(83) 9 1234-5678";
        var useCase = CreateUseCase(policyholder);

        await useCase.ExecuteAsync(policyholder.Id, request);

        policyholder.Name.ShouldBe(request.Nome.Trim());
        policyholder.Email.ShouldBe("jane.updated@example.com");
        policyholder.BirthDate.ShouldBe(request.DataNascimento);
        policyholder.Phone.ShouldBe("+5583912345678");
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoAntesDaBusca_Quando_EmailForInvalido))]
    [Trait("Application", "Policyholders - Update")]
    public async Task Deve_LancarExcecaoAntesDaBusca_Quando_EmailForInvalido()
    {
        await ShouldFailBeforeSearching(
            RequestUpdatePolicyholderJsonBuilder.Build(email: "invalid"),
            ResourcesMessageException.INVALID_EMAIL);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecaoAntesDaBusca_Quando_NomeForVazio))]
    [Trait("Application", "Policyholders - Update")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Deve_LancarExcecaoAntesDaBusca_Quando_NomeForVazio(string? name)
    {
        var request = RequestUpdatePolicyholderJsonBuilder.Build();
        request.Nome = name!;

        await ShouldFailBeforeSearching(request, ResourcesMessageException.POLICYHOLDER_NAME_REQUIRED);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoAntesDaBusca_Quando_DataDeNascimentoNaoForInformada))]
    [Trait("Application", "Policyholders - Update")]
    public async Task Deve_LancarExcecaoAntesDaBusca_Quando_DataDeNascimentoNaoForInformada()
    {
        var request = RequestUpdatePolicyholderJsonBuilder.Build();
        request.DataNascimento = default;

        await ShouldFailBeforeSearching(request, ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_REQUIRED);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoAntesDaBusca_Quando_DataDeNascimentoForFutura))]
    [Trait("Application", "Policyholders - Update")]
    public async Task Deve_LancarExcecaoAntesDaBusca_Quando_DataDeNascimentoForFutura()
    {
        var request = RequestUpdatePolicyholderJsonBuilder.Build();
        request.DataNascimento = BusinessClockBuilder.DefaultToday.AddDays(1);

        await ShouldFailBeforeSearching(request, ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_IN_FUTURE);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecaoAntesDaBusca_Quando_TelefoneForVazio))]
    [Trait("Application", "Policyholders - Update")]
    [InlineData(null)]
    [InlineData("")]
    public async Task Deve_LancarExcecaoAntesDaBusca_Quando_TelefoneForVazio(string? phone)
    {
        var request = RequestUpdatePolicyholderJsonBuilder.Build();
        request.Telefone = phone!;

        await ShouldFailBeforeSearching(request, ResourcesMessageException.POLICYHOLDER_PHONE_REQUIRED);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecaoAntesDaBusca_Quando_TelefoneForInvalido))]
    [Trait("Application", "Policyholders - Update")]
    [InlineData("123")]
    [InlineData("(83) 9 8647-16871")]
    public async Task Deve_LancarExcecaoAntesDaBusca_Quando_TelefoneForInvalido(string phone)
    {
        var request = RequestUpdatePolicyholderJsonBuilder.Build();
        request.Telefone = phone;

        await ShouldFailBeforeSearching(request, ResourcesMessageException.INVALID_PHONE);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontrado_Quando_SeguradoNaoExistir))]
    [Trait("Application", "Policyholders - Update")]
    public async Task Deve_LancarNaoEncontrado_Quando_SeguradoNaoExistir()
    {
        var useCase = CreateUseCase();

        await Should.ThrowAsync<NotFoundException>(
            () => useCase.ExecuteAsync(Guid.NewGuid(), RequestUpdatePolicyholderJsonBuilder.Build()));
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoSemAlterar_Quando_EmailPertencerAOutroSeguradoAtivo))]
    [Trait("Application", "Policyholders - Update")]
    public async Task Deve_LancarExcecaoSemAlterar_Quando_EmailPertencerAOutroSeguradoAtivo()
    {
        var policyholder = PolicyholderBuilder.Build();
        var originalEmail = policyholder.Email;
        var request = RequestUpdatePolicyholderJsonBuilder.Build(email: "TAKEN@EXAMPLE.COM");
        var useCase = CreateUseCase(policyholder, emailAlreadyExists: true);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.ExecuteAsync(policyholder.Id, request));

        policyholder.Email.ShouldBe(originalEmail);
        exception.GetErrors().ShouldContain(ResourcesMessageException.POLICYHOLDER_ACTIVE_EMAIL_EXISTS);
    }

    private static async Task ShouldFailBeforeSearching(RequestUpdatePolicyholderJson request, string expectedError)
    {
        var useCase = CreateUseCase();

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.ExecuteAsync(Guid.NewGuid(), request));

        exception.GetErrors().ShouldBe([expectedError]);
    }

    private static UpdatePolicyholderUseCase CreateUseCase(
        Policyholder? policyholder = null,
        bool emailAlreadyExists = false)
    {
        var policyholdersReadOnlyRepositoryBuilder = new PolicyholdersReadOnlyRepositoryBuilder();
        var policyholdersUpdateOnlyRepositoryBuilder = new PolicyholdersUpdateOnlyRepositoryBuilder();

        if (policyholder is not null)
            policyholdersUpdateOnlyRepositoryBuilder.GetByIdForUpdate(policyholder);

        if (emailAlreadyExists)
            policyholdersReadOnlyRepositoryBuilder.ExistActiveWithEmail();

        return new UpdatePolicyholderUseCase(
            policyholdersReadOnlyRepositoryBuilder.Build(),
            policyholdersUpdateOnlyRepositoryBuilder.Build(),
            UnitOfWorkBuilder.Build(),
            BusinessClockBuilder.Build(),
            LoggerBuilder.Build<UpdatePolicyholderUseCase>());
    }
}
