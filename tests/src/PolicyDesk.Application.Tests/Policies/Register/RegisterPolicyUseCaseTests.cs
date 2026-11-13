using CommonTestUtilities.Entities;
using CommonTestUtilities.Loggers;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Repositories.Policies;
using CommonTestUtilities.Repositories.Policyholders;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Time;
using PolicyDesk.Application.Policies.Register;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Policies.Register;

public class RegisterPolicyUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_RegistrarApolice_Quando_RequisicaoForValida))]
    [Trait("Application", "Policies - Register")]
    public async Task Deve_RegistrarApolice_Quando_RequisicaoForValida()
    {
        var policyholder = PolicyholderBuilder.Build();
        var request = RequestPolicyJsonBuilder.Build(
            policyholder.Id,
            description: "  Home policy  ",
            coverageAmount: 10000,
            premiumAmount: 100);
        var useCase = CreateUseCase(policyholder);

        var result = await useCase.ExecuteAsync(request);

        result.Id.ShouldNotBe(Guid.Empty);
        // O segurado é alterado junto para que uma inativação concorrente seja detectada como conflito
        policyholder.UpdatedAt.ShouldNotBeNull();
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontrado_Quando_SeguradoNaoExistir))]
    [Trait("Application", "Policies - Register")]
    public async Task Deve_LancarNaoEncontrado_Quando_SeguradoNaoExistir()
    {
        var useCase = CreateUseCase();

        await Should.ThrowAsync<NotFoundException>(() => useCase.ExecuteAsync(RequestPolicyJsonBuilder.Build()));
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacao_Quando_SeguradoEstiverInativo))]
    [Trait("Application", "Policies - Register")]
    public async Task Deve_LancarValidacao_Quando_SeguradoEstiverInativo()
    {
        var policyholder = PolicyholderBuilder.Build();
        policyholder.Deactivate();
        var useCase = CreateUseCase(policyholder);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.ExecuteAsync(RequestPolicyJsonBuilder.Build(policyholder.Id)));

        exception.GetErrors().ShouldBe([ResourcesMessageException.POLICYHOLDER_INACTIVE]);
    }

    [Theory(DisplayName = nameof(Deve_LancarValidacaoAntesDaBusca_Quando_DadosObrigatoriosForemInvalidos))]
    [Trait("Application", "Policies - Register")]
    [InlineData("", 10000, 100, false)]
    [InlineData("Description", 0, 100, false)]
    [InlineData("Description", 10000, 0, false)]
    [InlineData("Description", 10000, 100, true)]
    public async Task Deve_LancarValidacaoAntesDaBusca_Quando_DadosObrigatoriosForemInvalidos(
        string description,
        decimal coverage,
        decimal premium,
        bool missingDates)
    {
        var request = RequestPolicyJsonBuilder.Build(
            Guid.NewGuid(),
            description,
            coverage,
            premium,
            missingDates ? default : BusinessClockBuilder.DefaultToday,
            missingDates ? default : BusinessClockBuilder.DefaultToday.AddDays(1));
        var useCase = CreateUseCase();

        await Should.ThrowAsync<ErrorOnValidationException>(() => useCase.ExecuteAsync(request));
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoAntesDaBusca_Quando_DescricaoExcederOLimite))]
    [Trait("Application", "Policies - Register")]
    public async Task Deve_LancarValidacaoAntesDaBusca_Quando_DescricaoExcederOLimite()
    {
        var request = RequestPolicyJsonBuilder.Build(description: new string('a', FieldLimits.Description + 1));
        var useCase = CreateUseCase();

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(() => useCase.ExecuteAsync(request));

        exception.GetErrors().ShouldBe([FieldLimits.DescriptionTooLong]);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacao_Quando_DataFinalNaoForPosteriorAInicial))]
    [Trait("Application", "Policies - Register")]
    public async Task Deve_LancarValidacao_Quando_DataFinalNaoForPosteriorAInicial()
    {
        var today = BusinessClockBuilder.DefaultToday;
        var request = RequestPolicyJsonBuilder.Build(startDate: today, endDate: today);
        var useCase = CreateUseCase();

        await Should.ThrowAsync<ErrorOnValidationException>(() => useCase.ExecuteAsync(request));
    }

    private static RegisterPolicyUseCase CreateUseCase(Policyholder? policyholder = null)
    {
        var policyholdersUpdateOnlyRepositoryBuilder = new PolicyholdersUpdateOnlyRepositoryBuilder();

        if (policyholder is not null)
            policyholdersUpdateOnlyRepositoryBuilder.GetByIdIncludingInactive(policyholder);

        return new RegisterPolicyUseCase(
            PoliciesWriteOnlyRepositoryBuilder.Build(),
            policyholdersUpdateOnlyRepositoryBuilder.Build(),
            UnitOfWorkBuilder.Build(),
            LoggerBuilder.Build<RegisterPolicyUseCase>());
    }
}
