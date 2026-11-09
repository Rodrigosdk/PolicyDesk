using PolicyDesk.Domain.Validations;
using Microsoft.Extensions.Logging.Abstractions;
using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Time;
using Moq;
using PolicyDesk.Application.Policies.Register;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Policies;
using PolicyDesk.Domain.Repositories.Policyholders;
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
        var (useCase, writeRepository, unitOfWork, _) = CreateUseCase(policyholder);

        var result = await useCase.ExecuteAsync(request);

        result.Id.ShouldNotBe(Guid.Empty);
        writeRepository.Verify(repository => repository.AddAsync(It.Is<Policy>(policy =>
            policy.PolicyholderId == policyholder.Id &&
            policy.Description == "Home policy" &&
            policy.CoverageAmount == request.ValorCobertura &&
            policy.PremiumAmount == request.ValorPremio &&
            policy.StartDate == request.InicioVigencia &&
            policy.EndDate == request.FimVigencia)), Times.Once);
        unitOfWork.Verify(work => work.CommitAsync(), Times.Once);
        // O segurado é alterado junto para que uma inativação concorrente seja detectada como conflito
        policyholder.UpdatedAt.ShouldNotBeNull();
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontradoSemSalvar_Quando_SeguradoNaoExistir))]
    [Trait("Application", "Policies - Register")]
    public async Task Deve_LancarNaoEncontradoSemSalvar_Quando_SeguradoNaoExistir()
    {
        var (useCase, writeRepository, unitOfWork, _) = CreateUseCase();

        await Should.ThrowAsync<NotFoundException>(
            () => useCase.ExecuteAsync(RequestPolicyJsonBuilder.Build()));

        writeRepository.Verify(repository => repository.AddAsync(It.IsAny<Policy>()), Times.Never);
        unitOfWork.Verify(work => work.CommitAsync(), Times.Never);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoSemSalvar_Quando_SeguradoEstiverInativo))]
    [Trait("Application", "Policies - Register")]
    public async Task Deve_LancarValidacaoSemSalvar_Quando_SeguradoEstiverInativo()
    {
        var policyholder = PolicyholderBuilder.Build();
        policyholder.Deactivate();
        var (useCase, writeRepository, unitOfWork, _) = CreateUseCase(policyholder);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.ExecuteAsync(RequestPolicyJsonBuilder.Build(policyholder.Id)));

        exception.GetErrors().ShouldBe([ResourcesMessageException.POLICYHOLDER_INACTIVE]);
        writeRepository.Verify(repository => repository.AddAsync(It.IsAny<Policy>()), Times.Never);
        unitOfWork.Verify(work => work.CommitAsync(), Times.Never);
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
        var (useCase, writeRepository, unitOfWork, policyholdersRepository) = CreateUseCase();
        var request = RequestPolicyJsonBuilder.Build(
            Guid.NewGuid(),
            description,
            coverage,
            premium,
            missingDates ? default : BusinessClockBuilder.DefaultToday,
            missingDates ? default : BusinessClockBuilder.DefaultToday.AddDays(1));

        await Should.ThrowAsync<ErrorOnValidationException>(() => useCase.ExecuteAsync(request));

        writeRepository.Verify(repository => repository.AddAsync(It.IsAny<Policy>()), Times.Never);
        unitOfWork.Verify(work => work.CommitAsync(), Times.Never);
        policyholdersRepository.Verify(repository => repository.GetByIdIncludingInactiveAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacao_Quando_DataFinalNaoForPosteriorAInicial))]
    [Trait("Application", "Policies - Register")]
    public async Task Deve_LancarValidacao_Quando_DataFinalNaoForPosteriorAInicial()
    {
        var today = BusinessClockBuilder.DefaultToday;
        var (useCase, writeRepository, unitOfWork, _) = CreateUseCase();
        var request = RequestPolicyJsonBuilder.Build(startDate: today, endDate: today);

        await Should.ThrowAsync<ErrorOnValidationException>(() => useCase.ExecuteAsync(request));

        writeRepository.Verify(repository => repository.AddAsync(It.IsAny<Policy>()), Times.Never);
        unitOfWork.Verify(work => work.CommitAsync(), Times.Never);
    }

    private static (
        RegisterPolicyUseCase UseCase,
        Mock<IPoliciesWriteOnlyRepository> WriteRepository,
        Mock<IUnitOfWork> UnitOfWork,
        Mock<IPolicyholdersUpdateOnlyRepository> PolicyholdersRepository)
        CreateUseCase(Policyholder? policyholder = null)
    {
        var writeRepository = PoliciesWriteOnlyRepositoryBuilder.Build();
        var policyholdersRepository = new PolicyholdersUpdateOnlyRepositoryBuilder();
        if (policyholder is not null)
            policyholdersRepository.GetByIdIncludingInactive(policyholder);
        var unitOfWork = UnitOfWorkBuilder.Build();
        return (
            new RegisterPolicyUseCase(writeRepository.Object, policyholdersRepository.Build(), unitOfWork.Object, NullLogger<RegisterPolicyUseCase>.Instance),
            writeRepository,
            unitOfWork,
            policyholdersRepository.Mock);
    }


    [Fact(DisplayName = nameof(Deve_LancarValidacaoAntesDaBusca_Quando_DescricaoExcederOLimite))]
    [Trait("Application", "Policies - Register")]
    public async Task Deve_LancarValidacaoAntesDaBusca_Quando_DescricaoExcederOLimite()
    {
        var (useCase, writeRepository, unitOfWork, policyholdersRepository) = CreateUseCase();
        var request = RequestPolicyJsonBuilder.Build(description: new string('a', FieldLimits.Description + 1));

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(() => useCase.ExecuteAsync(request));

        exception.GetErrors().ShouldBe([FieldLimits.DescriptionTooLong]);
        writeRepository.Verify(repository => repository.AddAsync(It.IsAny<Policy>()), Times.Never);
        unitOfWork.Verify(work => work.CommitAsync(), Times.Never);
        policyholdersRepository.Verify(repository => repository.GetByIdIncludingInactiveAsync(It.IsAny<Guid>()), Times.Never);
    }
}
