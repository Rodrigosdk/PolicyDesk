using Microsoft.Extensions.Logging.Abstractions;
using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Time;
using Moq;
using PolicyDesk.Application.Policyholders.Update;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;
using PolicyDesk.Domain.Entities;

namespace PolicyDesk.Application.Tests.Policyholders.Update;

public class UpdatePolicyholderUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_AtualizarSeguradoEConfirmar_Quando_RequisicaoForValida))]
    [Trait("Application", "Policyholders - Update")]
    public async Task Deve_AtualizarSeguradoEConfirmar_Quando_RequisicaoForValida()
    {
        var policyholder = PolicyholderBuilder.Build();
        var (command, readBuilder, _, unitOfWork) = CreateCommand(policyholder);
        var request = RequestUpdatePolicyholderJsonBuilder.Build(email: "Jane.Updated@Example.com");
        request.Telefone = "(83) 9 1234-5678";

        await command.ExecuteAsync(policyholder.Id, request);

        policyholder.Name.ShouldBe(request.Nome.Trim());
        policyholder.Email.ShouldBe("jane.updated@example.com");
        policyholder.BirthDate.ShouldBe(request.DataNascimento);
        policyholder.Phone.ShouldBe("+5583912345678");
        unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        readBuilder.Mock.Verify(
            r => r.ExistActivePolicyholdersWithEmailAsync("jane.updated@example.com", policyholder.Id),
            Times.Once);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoAntesDaBusca_Quando_EmailForInvalido))]
    [Trait("Application", "Policyholders - Update")]
    public async Task Deve_LancarExcecaoAntesDaBusca_Quando_EmailForInvalido()
    {
        var (command, _, updateBuilder, unitOfWork) = CreateCommand();

        await Should.ThrowAsync<ErrorOnValidationException>(
            () => command.ExecuteAsync(Guid.NewGuid(), RequestUpdatePolicyholderJsonBuilder.Build(email: "invalid")));

        updateBuilder.Mock.Verify(r => r.GetByIdForUpdateAsync(It.IsAny<Guid>()), Times.Never);
        unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
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

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontradoSemConfirmar_Quando_SeguradoNaoExistir))]
    [Trait("Application", "Policyholders - Update")]
    public async Task Deve_LancarNaoEncontradoSemConfirmar_Quando_SeguradoNaoExistir()
    {
        var (command, readBuilder, _, unitOfWork) = CreateCommand();

        await Should.ThrowAsync<NotFoundException>(
            () => command.ExecuteAsync(Guid.NewGuid(), RequestUpdatePolicyholderJsonBuilder.Build()));

        readBuilder.Mock.Verify(
            r => r.ExistActivePolicyholdersWithEmailAsync(It.IsAny<string>(), It.IsAny<Guid?>()),
            Times.Never);
        unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoSemConfirmar_Quando_EmailPertencerAOutroSeguradoAtivo))]
    [Trait("Application", "Policyholders - Update")]
    public async Task Deve_LancarExcecaoSemConfirmar_Quando_EmailPertencerAOutroSeguradoAtivo()
    {
        var policyholder = PolicyholderBuilder.Build();
        var originalEmail = policyholder.Email;
        var (command, readBuilder, _, unitOfWork) = CreateCommand(policyholder, emailAlreadyExists: true);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => command.ExecuteAsync(
                policyholder.Id,
                RequestUpdatePolicyholderJsonBuilder.Build(email: "TAKEN@EXAMPLE.COM")));

        policyholder.Email.ShouldBe(originalEmail);
        exception.GetErrors().ShouldContain(ResourcesMessageException.POLICYHOLDER_ACTIVE_EMAIL_EXISTS);
        unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        readBuilder.Mock.Verify(
            r => r.ExistActivePolicyholdersWithEmailAsync("taken@example.com", policyholder.Id),
            Times.Once);
    }

    private static async Task ShouldFailBeforeSearching(RequestUpdatePolicyholderJson request, string expectedError)
    {
        var (command, _, updateBuilder, unitOfWork) = CreateCommand();

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => command.ExecuteAsync(Guid.NewGuid(), request));

        exception.GetErrors().ShouldBe([expectedError]);
        updateBuilder.Mock.Verify(r => r.GetByIdForUpdateAsync(It.IsAny<Guid>()), Times.Never);
        unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    private static (UpdatePolicyholderUseCase Command, PolicyholdersReadOnlyRepositoryBuilder ReadBuilder,
        PolicyholdersUpdateOnlyRepositoryBuilder UpdateBuilder, Mock<IUnitOfWork> UnitOfWork)
        CreateCommand(Policyholder? policyholder = null, bool emailAlreadyExists = false)
    {
        var readBuilder = new PolicyholdersReadOnlyRepositoryBuilder();
        var updateBuilder = new PolicyholdersUpdateOnlyRepositoryBuilder();
        if (policyholder is not null)
            updateBuilder.GetByIdForUpdate(policyholder);
        if (emailAlreadyExists)
            readBuilder.ExistActiveWithEmail();

        var unitOfWork = UnitOfWorkBuilder.Build();

        return (new UpdatePolicyholderUseCase(readBuilder.Build(), updateBuilder.Build(), unitOfWork.Object, BusinessClockBuilder.Build(), NullLogger<UpdatePolicyholderUseCase>.Instance), readBuilder, updateBuilder, unitOfWork);
    }
}
