using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using Moq;
using PolicyDesk.Application.Claims.Register;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Claims;
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
        var request = RequestClaimJsonBuilder.Build("  Broken window  ", 500, DateTime.UtcNow.Date);
        var (useCase, writeRepository, unitOfWork) = CreateUseCase(policy);

        var result = await useCase.Execute(policy.Id, request);

        result.Id.ShouldNotBe(Guid.Empty);
        writeRepository.Verify(repository => repository.Add(It.Is<Claim>(claim =>
            claim.PolicyId == policy.Id &&
            claim.Description == "Broken window" &&
            claim.Amount == 500 &&
            claim.OccurredOn == request.OccurredOn.Date)), Times.Once);
        unitOfWork.Verify(work => work.Commit(), Times.Once);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontradoSemSalvar_Quando_ApoliceNaoExistir))]
    [Trait("Application", "Claims - Register")]
    public async Task Deve_LancarNaoEncontradoSemSalvar_Quando_ApoliceNaoExistir()
    {
        var (useCase, writeRepository, unitOfWork) = CreateUseCase();

        await Should.ThrowAsync<NotFoundException>(
            () => useCase.Execute(Guid.NewGuid(), RequestClaimJsonBuilder.Build()));

        VerifyNothingSaved(writeRepository, unitOfWork);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoSemSalvar_Quando_ApoliceEstiverCancelada))]
    [Trait("Application", "Claims - Register")]
    public async Task Deve_LancarValidacaoSemSalvar_Quando_ApoliceEstiverCancelada()
    {
        var policy = PolicyBuilder.Build();
        policy.Cancel();
        var (useCase, writeRepository, unitOfWork) = CreateUseCase(policy);

        await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.Execute(policy.Id, RequestClaimJsonBuilder.Build()));

        VerifyNothingSaved(writeRepository, unitOfWork);
    }

    [Theory(DisplayName = nameof(Deve_LancarValidacaoSemSalvar_Quando_DataEstiverForaDaVigencia))]
    [Trait("Application", "Claims - Register")]
    [InlineData(-40)]
    [InlineData(-5)]
    public async Task Deve_LancarValidacaoSemSalvar_Quando_DataEstiverForaDaVigencia(int dayOffset)
    {
        // Vigência inteira no passado: -40 cai antes do início e -5 depois do fim, ambos sem ser data futura
        var today = DateTime.UtcNow.Date;
        var policy = PolicyBuilder.Build(startDate: today.AddDays(-30), endDate: today.AddDays(-10));
        var (useCase, writeRepository, unitOfWork) = CreateUseCase(policy);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.Execute(policy.Id, RequestClaimJsonBuilder.Build(occurredOn: today.AddDays(dayOffset))));

        exception.GetErrors().ShouldBe([ResourcesMessageException.CLAIM_DATE_OUTSIDE_POLICY_PERIOD]);
        VerifyNothingSaved(writeRepository, unitOfWork);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoAntesDaBusca_Quando_DataDeOcorrenciaForFutura))]
    [Trait("Application", "Claims - Register")]
    public async Task Deve_LancarValidacaoAntesDaBusca_Quando_DataDeOcorrenciaForFutura()
    {
        var policiesRepository = new PoliciesReadOnlyRepositoryBuilder();
        var writeRepository = ClaimsWriteOnlyRepositoryBuilder.Build();
        var unitOfWork = UnitOfWorkBuilder.Build();
        var useCase = new RegisterClaimUseCase(writeRepository.Object, policiesRepository.Build(), unitOfWork.Object);
        var request = RequestClaimJsonBuilder.Build(occurredOn: DateTime.UtcNow.Date.AddDays(1));

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(() => useCase.Execute(Guid.NewGuid(), request));

        exception.GetErrors().ShouldBe([ResourcesMessageException.CLAIM_OCCURRENCE_DATE_IN_FUTURE]);
        policiesRepository.Mock.Verify(repository => repository.GetById(It.IsAny<Guid>()), Times.Never);
        VerifyNothingSaved(writeRepository, unitOfWork);
    }

    [Fact(DisplayName = nameof(Deve_RegistrarSinistro_Quando_OcorrenciaForHoje))]
    [Trait("Application", "Claims - Register")]
    public async Task Deve_RegistrarSinistro_Quando_OcorrenciaForHoje()
    {
        var today = DateTime.UtcNow.Date;
        var policy = PolicyBuilder.Build(startDate: today.AddDays(-1), endDate: today.AddDays(30));
        var (useCase, writeRepository, unitOfWork) = CreateUseCase(policy);

        await useCase.Execute(policy.Id, RequestClaimJsonBuilder.Build(occurredOn: today));

        writeRepository.Verify(repository => repository.Add(It.Is<Claim>(claim => claim.OccurredOn == today)), Times.Once);
        unitOfWork.Verify(work => work.Commit(), Times.Once);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoSemSalvar_Quando_ValorExcederCobertura))]
    [Trait("Application", "Claims - Register")]
    public async Task Deve_LancarValidacaoSemSalvar_Quando_ValorExcederCobertura()
    {
        var policy = PolicyBuilder.Build(coverageAmount: 1000);
        var (useCase, writeRepository, unitOfWork) = CreateUseCase(policy);

        await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.Execute(policy.Id, RequestClaimJsonBuilder.Build(amount: 1000.01m)));

        VerifyNothingSaved(writeRepository, unitOfWork);
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
        var policiesRepository = new PoliciesReadOnlyRepositoryBuilder();
        var writeRepository = ClaimsWriteOnlyRepositoryBuilder.Build();
        var unitOfWork = UnitOfWorkBuilder.Build();
        var useCase = new RegisterClaimUseCase(writeRepository.Object, policiesRepository.Build(), unitOfWork.Object);
        var request = RequestClaimJsonBuilder.Build(description, amount, missingDate ? default(DateTime) : DateTime.UtcNow.Date);

        await Should.ThrowAsync<ErrorOnValidationException>(() => useCase.Execute(Guid.NewGuid(), request));

        policiesRepository.Mock.Verify(repository => repository.GetById(It.IsAny<Guid>()), Times.Never);
        VerifyNothingSaved(writeRepository, unitOfWork);
    }

    private static void VerifyNothingSaved(Mock<IClaimsWriteOnlyRepository> writeRepository, Mock<IUnitOfWork> unitOfWork)
    {
        writeRepository.Verify(repository => repository.Add(It.IsAny<Claim>()), Times.Never);
        unitOfWork.Verify(work => work.Commit(), Times.Never);
    }

    private static (
        RegisterClaimUseCase UseCase,
        Mock<IClaimsWriteOnlyRepository> WriteRepository,
        Mock<IUnitOfWork> UnitOfWork)
        CreateUseCase(Policy? policy = null)
    {
        var policiesRepository = new PoliciesReadOnlyRepositoryBuilder();
        if (policy is not null)
            policiesRepository.GetById(policy);
        var writeRepository = ClaimsWriteOnlyRepositoryBuilder.Build();
        var unitOfWork = UnitOfWorkBuilder.Build();
        return (
            new RegisterClaimUseCase(writeRepository.Object, policiesRepository.Build(), unitOfWork.Object),
            writeRepository,
            unitOfWork);
    }
}
