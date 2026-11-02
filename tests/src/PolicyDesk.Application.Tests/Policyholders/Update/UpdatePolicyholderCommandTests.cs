using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using Moq;
using PolicyDesk.Application.Policyholders.Update;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;
using Policyholder = PolicyDesk.Domain.Entities.Policyholders;

namespace PolicyDesk.Application.Tests.Policyholders.Update;

public class UpdatePolicyholderCommandTests
{
    [Fact(DisplayName = nameof(Deve_AtualizarSeguradoEConfirmar_Quando_RequisicaoForValida))]
    [Trait("Application", "Policyholders - Update")]
    public async Task Deve_AtualizarSeguradoEConfirmar_Quando_RequisicaoForValida()
    {
        var policyholder = PolicyholderBuilder.Build();
        var (command, readBuilder, unitOfWork) = CreateCommand(policyholder);
        var request = RequestPolicyholdersUpdateJsonBuilder.Build(email: "Jane.Updated@Example.com");

        await command.Execute(policyholder.Id, request);

        policyholder.Name.ShouldBe(request.Name.Trim());
        policyholder.Email.ShouldBe("jane.updated@example.com");
        policyholder.BirthDate.ShouldBe(request.BirthDate);
        policyholder.Phone.ShouldBe(request.PhoneNumber);
        unitOfWork.Verify(u => u.Commit(), Times.Once);
        readBuilder.Mock.Verify(
            r => r.ExistActivePolicyholdersWithEmail("jane.updated@example.com", policyholder.Id),
            Times.Once);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoAntesDaBusca_Quando_EmailForInvalido))]
    [Trait("Application", "Policyholders - Update")]
    public async Task Deve_LancarExcecaoAntesDaBusca_Quando_EmailForInvalido()
    {
        var (command, readBuilder, unitOfWork) = CreateCommand();

        await Should.ThrowAsync<ErrorOnValidationException>(
            () => command.Execute(Guid.NewGuid(), RequestPolicyholdersUpdateJsonBuilder.Build(email: "invalid")));

        readBuilder.Mock.Verify(r => r.GetById(It.IsAny<Guid>()), Times.Never);
        unitOfWork.Verify(u => u.Commit(), Times.Never);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecaoAntesDaBusca_Quando_NomeForVazio))]
    [Trait("Application", "Policyholders - Update")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Deve_LancarExcecaoAntesDaBusca_Quando_NomeForVazio(string? name)
    {
        var request = RequestPolicyholdersUpdateJsonBuilder.Build();
        request.Name = name!;

        await ShouldFailBeforeSearching(request, ResourcesMessageException.POLICYHOLDER_NAME_REQUIRED);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoAntesDaBusca_Quando_DataDeNascimentoNaoForInformada))]
    [Trait("Application", "Policyholders - Update")]
    public async Task Deve_LancarExcecaoAntesDaBusca_Quando_DataDeNascimentoNaoForInformada()
    {
        var request = RequestPolicyholdersUpdateJsonBuilder.Build();
        request.BirthDate = default;

        await ShouldFailBeforeSearching(request, ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_REQUIRED);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoAntesDaBusca_Quando_DataDeNascimentoForFutura))]
    [Trait("Application", "Policyholders - Update")]
    public async Task Deve_LancarExcecaoAntesDaBusca_Quando_DataDeNascimentoForFutura()
    {
        var request = RequestPolicyholdersUpdateJsonBuilder.Build();
        request.BirthDate = DateTime.UtcNow.Date.AddDays(1);

        await ShouldFailBeforeSearching(request, ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_IN_FUTURE);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecaoAntesDaBusca_Quando_TelefoneForVazio))]
    [Trait("Application", "Policyholders - Update")]
    [InlineData(null)]
    [InlineData("")]
    public async Task Deve_LancarExcecaoAntesDaBusca_Quando_TelefoneForVazio(string? phone)
    {
        var request = RequestPolicyholdersUpdateJsonBuilder.Build();
        request.PhoneNumber = phone!;

        await ShouldFailBeforeSearching(request, ResourcesMessageException.POLICYHOLDER_PHONE_REQUIRED);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecaoAntesDaBusca_Quando_TelefoneForInvalido))]
    [Trait("Application", "Policyholders - Update")]
    [InlineData("123")]
    [InlineData("(83) 9 8647-16871")]
    public async Task Deve_LancarExcecaoAntesDaBusca_Quando_TelefoneForInvalido(string phone)
    {
        var request = RequestPolicyholdersUpdateJsonBuilder.Build();
        request.PhoneNumber = phone;

        await ShouldFailBeforeSearching(request, ResourcesMessageException.INVALID_PHONE);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontradoSemConfirmar_Quando_SeguradoNaoExistir))]
    [Trait("Application", "Policyholders - Update")]
    public async Task Deve_LancarNaoEncontradoSemConfirmar_Quando_SeguradoNaoExistir()
    {
        var (command, readBuilder, unitOfWork) = CreateCommand();

        await Should.ThrowAsync<NotFoundException>(
            () => command.Execute(Guid.NewGuid(), RequestPolicyholdersUpdateJsonBuilder.Build()));

        readBuilder.Mock.Verify(
            r => r.ExistActivePolicyholdersWithEmail(It.IsAny<string>(), It.IsAny<Guid?>()),
            Times.Never);
        unitOfWork.Verify(u => u.Commit(), Times.Never);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoSemConfirmar_Quando_EmailPertencerAOutroSeguradoAtivo))]
    [Trait("Application", "Policyholders - Update")]
    public async Task Deve_LancarExcecaoSemConfirmar_Quando_EmailPertencerAOutroSeguradoAtivo()
    {
        var policyholder = PolicyholderBuilder.Build();
        var originalEmail = policyholder.Email;
        var (command, readBuilder, unitOfWork) = CreateCommand(policyholder, emailAlreadyExists: true);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => command.Execute(
                policyholder.Id,
                RequestPolicyholdersUpdateJsonBuilder.Build(email: "TAKEN@EXAMPLE.COM")));

        policyholder.Email.ShouldBe(originalEmail);
        exception.GetErrors().ShouldContain(ResourcesMessageException.POLICYHOLDER_ACTIVE_EMAIL_EXISTS);
        unitOfWork.Verify(u => u.Commit(), Times.Never);
        readBuilder.Mock.Verify(
            r => r.ExistActivePolicyholdersWithEmail("taken@example.com", policyholder.Id),
            Times.Once);
    }

    private static async Task ShouldFailBeforeSearching(RequestPolicyholdersUpdateJson request, string expectedError)
    {
        var (command, readBuilder, unitOfWork) = CreateCommand();

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => command.Execute(Guid.NewGuid(), request));

        exception.GetErrors().ShouldBe([expectedError]);
        readBuilder.Mock.Verify(r => r.GetById(It.IsAny<Guid>()), Times.Never);
        unitOfWork.Verify(u => u.Commit(), Times.Never);
    }

    private static (UpdatePolicyholderCommand Command, PolicyholdersReadOnlyRepositoryBuilder ReadBuilder, Mock<IUnitOfWork> UnitOfWork)
        CreateCommand(Policyholder? policyholder = null, bool emailAlreadyExists = false)
    {
        var readBuilder = new PolicyholdersReadOnlyRepositoryBuilder();
        if (policyholder is not null)
            readBuilder.GetById(policyholder);
        if (emailAlreadyExists)
            readBuilder.ExistActiveWithEmail();

        var unitOfWork = UnitOfWorkBuilder.Build();

        return (new UpdatePolicyholderCommand(readBuilder.Build(), unitOfWork.Object), readBuilder, unitOfWork);
    }
}
