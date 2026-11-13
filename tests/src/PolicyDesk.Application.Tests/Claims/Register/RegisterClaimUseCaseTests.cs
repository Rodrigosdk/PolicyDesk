using CommonTestUtilities.Entities;
using CommonTestUtilities.Loggers;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Repositories.Claims;
using CommonTestUtilities.Repositories.Policies;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Time;
using PolicyDesk.Application.Claims.Register;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Claims.Register;

public class RegisterClaimUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_RegistrarSinistro_Quando_RequisicaoForValida))]
    [Trait("Application", "Claims - Register")]
    public async Task Deve_RegistrarSinistro_Quando_RequisicaoForValida()
    {
        var policy = PolicyBuilder.Build(coverageAmount: 10000);
        var request = RequestClaimJsonBuilder.Build("  Broken window  ", 500, BusinessClockBuilder.DefaultToday);
        var useCase = CreateUseCase(policy);

        var result = await useCase.ExecuteAsync(policy.Id, request);

        result.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontrado_Quando_ApoliceNaoExistir))]
    [Trait("Application", "Claims - Register")]
    public async Task Deve_LancarNaoEncontrado_Quando_ApoliceNaoExistir()
    {
        var useCase = CreateUseCase();

        await useCase.ExecuteAsync(Guid.NewGuid(), RequestClaimJsonBuilder.Build()).ShouldThrowAsync<NotFoundException>();
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacao_Quando_ApoliceEstiverCancelada))]
    [Trait("Application", "Claims - Register")]
    public async Task Deve_LancarValidacao_Quando_ApoliceEstiverCancelada()
    {
        var policy = PolicyBuilder.Build();
        policy.Cancel();
        var useCase = CreateUseCase(policy);

        await useCase.ExecuteAsync(policy.Id, RequestClaimJsonBuilder.Build()).ShouldThrowAsync<ErrorOnValidationException>();
    }

    [Theory(DisplayName = nameof(Deve_LancarValidacao_Quando_DataEstiverForaDaVigencia))]
    [Trait("Application", "Claims - Register")]
    [InlineData(-40)]
    [InlineData(-5)]
    public async Task Deve_LancarValidacao_Quando_DataEstiverForaDaVigencia(int dayOffset)
    {
        // Vigência inteira no passado: -40 cai antes do início e -5 depois do fim, ambos sem ser data futura
        var today = BusinessClockBuilder.DefaultToday;
        var policy = PolicyBuilder.Build(startDate: today.AddDays(-30), endDate: today.AddDays(-10));
        var useCase = CreateUseCase(policy);

        var exception = await useCase.ExecuteAsync(policy.Id, RequestClaimJsonBuilder.Build(occurredOn: today.AddDays(dayOffset))).ShouldThrowAsync<ErrorOnValidationException>();
        exception.GetErrors().ShouldSatisfyAllConditions(errorMessage =>
        {
            errorMessage.Count.ShouldBe(1);
            errorMessage.ShouldContain(ResourcesMessageException.CLAIM_DATE_OUTSIDE_POLICY_PERIOD);
        });
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoAntesDaBusca_Quando_DataDeOcorrenciaForFutura))]
    [Trait("Application", "Claims - Register")]
    public async Task Deve_LancarValidacaoAntesDaBusca_Quando_DataDeOcorrenciaForFutura()
    {
        var request = RequestClaimJsonBuilder.Build(occurredOn: BusinessClockBuilder.DefaultToday.AddDays(1));
        var useCase = CreateUseCase();

        var exception = await useCase.ExecuteAsync(Guid.NewGuid(), request).ShouldThrowAsync<ErrorOnValidationException>();
        exception.GetErrors().ShouldSatisfyAllConditions(errorMessage =>
        {
            errorMessage.Count.ShouldBe(1);
            errorMessage.ShouldContain(ResourcesMessageException.CLAIM_OCCURRENCE_DATE_IN_FUTURE);
        });
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacao_Quando_OcorrenciaForAmanhaNoBrasilMasHojeEmUtc))]
    [Trait("Application", "Claims - Register")]
    public async Task Deve_LancarValidacao_Quando_OcorrenciaForAmanhaNoBrasilMasHojeEmUtc()
    {
        var policy = PolicyBuilder.Build();
        var request = RequestClaimJsonBuilder.Build(occurredOn: new DateOnly(2026, 3, 11));
        var useCase = CreateUseCase(policy, clock: BusinessClockBuilder.Build(BusinessClockBuilder.LateEveningUtcNow));

        var exception = await useCase.ExecuteAsync(policy.Id, request).ShouldThrowAsync<ErrorOnValidationException>();
        exception.GetErrors().ShouldSatisfyAllConditions(errorMessage =>
        {
            errorMessage.Count.ShouldBe(1);
            errorMessage.ShouldContain(ResourcesMessageException.CLAIM_OCCURRENCE_DATE_IN_FUTURE);
        });
    }

    [Fact(DisplayName = nameof(Deve_RegistrarSinistro_Quando_OcorrenciaForHojeNoBrasilNoUltimoDiaDeVigencia))]
    [Trait("Application", "Claims - Register")]
    public async Task Deve_RegistrarSinistro_Quando_OcorrenciaForHojeNoBrasilNoUltimoDiaDeVigencia()
    {
        var today = BusinessClockBuilder.DefaultToday;
        var policy = PolicyBuilder.Build(coverageAmount: 10000, startDate: today.AddDays(-30), endDate: today);
        var request = RequestClaimJsonBuilder.Build(amount: 500, occurredOn: today);
        var useCase = CreateUseCase(policy, clock: BusinessClockBuilder.Build(BusinessClockBuilder.LateEveningUtcNow));

        var result = await useCase.ExecuteAsync(policy.Id, request);

        result.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact(DisplayName = nameof(Deve_RegistrarSinistro_Quando_OcorrenciaForHoje))]
    [Trait("Application", "Claims - Register")]
    public async Task Deve_RegistrarSinistro_Quando_OcorrenciaForHoje()
    {
        var today = BusinessClockBuilder.DefaultToday;
        // Valores fixos: com cobertura e valor aleatórios o sinistro podia exceder a cobertura
        var policy = PolicyBuilder.Build(coverageAmount: 10000, startDate: today.AddDays(-1), endDate: today.AddDays(30));
        var useCase = CreateUseCase(policy);

        var result = await useCase.ExecuteAsync(policy.Id, RequestClaimJsonBuilder.Build(amount: 500, occurredOn: today));

        result.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact(DisplayName = nameof(Deve_LancarRegra_Quando_ValorExcederCobertura))]
    [Trait("Application", "Claims - Register")]
    public async Task Deve_LancarRegra_Quando_ValorExcederCobertura()
    {
        var policy = PolicyBuilder.Build(coverageAmount: 1000);
        var useCase = CreateUseCase(policy);

        var exception = await useCase.ExecuteAsync(policy.Id, RequestClaimJsonBuilder.Build(amount: 1000.01m)).ShouldThrowAsync<DomainRuleException>();
        exception.Message.ShouldBe(ResourcesMessageException.CLAIM_AMOUNT_EXCEEDS_COVERAGE);
    }

    [Fact(DisplayName = nameof(Deve_LancarRegra_Quando_SomaDosSinistrosExcederCobertura))]
    [Trait("Application", "Claims - Register")]
    public async Task Deve_LancarRegra_Quando_SomaDosSinistrosExcederCobertura()
    {
        var policy = PolicyBuilder.Build(coverageAmount: 10000);
        var useCase = CreateUseCase(policy, committedAmount: 8000);

        var exception = await useCase.ExecuteAsync(policy.Id, RequestClaimJsonBuilder.Build(amount: 2000.01m)).ShouldThrowAsync<DomainRuleException>();
        exception.Message.ShouldBe(ResourcesMessageException.CLAIM_AMOUNT_EXCEEDS_COVERAGE);
    }

    [Fact(DisplayName = nameof(Deve_RegistrarSinistroEAtualizarApolice_Quando_ValorCompletarOSaldo))]
    [Trait("Application", "Claims - Register")]
    public async Task Deve_RegistrarSinistroEAtualizarApolice_Quando_ValorCompletarOSaldo()
    {
        var policy = PolicyBuilder.Build(coverageAmount: 10000);
        var useCase = CreateUseCase(policy, committedAmount: 8000);

        var result = await useCase.ExecuteAsync(policy.Id, RequestClaimJsonBuilder.Build(amount: 2000));

        result.Id.ShouldNotBe(Guid.Empty);
        policy.UpdatedAt.ShouldNotBeNull();
    }

    [Theory(DisplayName = nameof(Deve_LancarValidacaoAntesDaBusca_Quando_DadosObrigatoriosForemInvalidos))]
    [Trait("Application", "Claims - Register")]
    [InlineData("", 100, false)]
    [InlineData("   ", 100, false)]
    [InlineData("Description", 0, false)]
    [InlineData("Description", -1, false)]
    [InlineData("Description", 100, true)]
    public async Task Deve_LancarValidacaoAntesDaBusca_Quando_DadosObrigatoriosForemInvalidos(
        string description,
        decimal amount,
        bool missingDate)
    {
        var request = RequestClaimJsonBuilder.Build(description, amount, missingDate ? default(DateOnly) : BusinessClockBuilder.DefaultToday);
        var useCase = CreateUseCase();

        await useCase.ExecuteAsync(Guid.NewGuid(), request).ShouldThrowAsync<ErrorOnValidationException>();
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacao_Quando_DescricaoExcederOLimite))]
    [Trait("Application", "Claims - Register")]
    public async Task Deve_LancarValidacao_Quando_DescricaoExcederOLimite()
    {
        var policy = PolicyBuilder.Build();
        var request = RequestClaimJsonBuilder.Build(description: new string('a', FieldLimits.Description + 1));
        var useCase = CreateUseCase(policy);

        var exception = await useCase.ExecuteAsync(policy.Id, request).ShouldThrowAsync<ErrorOnValidationException>();
        exception.GetErrors().ShouldSatisfyAllConditions(errorMessage =>
        {
            errorMessage.Count.ShouldBe(1);
            errorMessage.ShouldContain(FieldLimits.DescriptionTooLong);
        });
    }

    private static RegisterClaimUseCase CreateUseCase(
        Policy? policy = null,
        decimal committedAmount = 0,
        BusinessClock? clock = null)
    {
        var claimsReadOnlyRepositoryBuilder = new ClaimsReadOnlyRepositoryBuilder();
        var policiesUpdateOnlyRepositoryBuilder = new PoliciesUpdateOnlyRepositoryBuilder();

        claimsReadOnlyRepositoryBuilder.SumCommittedAmount(committedAmount);

        if (policy is not null)
            policiesUpdateOnlyRepositoryBuilder.GetByIdForUpdate(policy);

        return new RegisterClaimUseCase(
            ClaimsWriteOnlyRepositoryBuilder.Build(),
            claimsReadOnlyRepositoryBuilder.Build(),
            policiesUpdateOnlyRepositoryBuilder.Build(),
            UnitOfWorkBuilder.Build(),
            clock ?? BusinessClockBuilder.Build(),
            LoggerBuilder.Build<RegisterClaimUseCase>());
    }
}
