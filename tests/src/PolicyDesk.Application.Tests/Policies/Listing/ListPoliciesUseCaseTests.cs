using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using Moq;
using PolicyDesk.Application.Policies.Listing;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Policies.Listing;

public class ListPoliciesUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_MapearApolicesParaResposta_Quando_Listar))]
    [Trait("Application", "Policies - Listing")]
    public async Task Deve_MapearApolicesParaResposta_Quando_Listar()
    {
        var policy = PolicyBuilder.Build();
        var useCase = CreateUseCase([policy]);

        var result = (await useCase.ListAsync(null, null)).ShouldHaveSingleItem();

        result.Id.ShouldBe(policy.Id);
        result.SeguradoId.ShouldBe(policy.PolicyholderId);
        result.Descricao.ShouldBe(policy.Description);
        result.ValorCobertura.ShouldBe(policy.CoverageAmount);
        result.ValorPremio.ShouldBe(policy.PremiumAmount);
        result.InicioVigencia.ShouldBe(policy.StartDate);
        result.FimVigencia.ShouldBe(policy.EndDate);
        result.Status.ShouldBe(policy.Status);
        result.CreatedAt.ShouldBe(policy.CreatedAt);
        result.UpdatedAt.ShouldBe(policy.UpdatedAt);
    }

    [Fact(DisplayName = nameof(Deve_EncaminharFiltros_Quando_Listar))]
    [Trait("Application", "Policies - Listing")]
    public async Task Deve_EncaminharFiltros_Quando_Listar()
    {
        var policyholderId = Guid.NewGuid();
        var builder = new PoliciesReadOnlyRepositoryBuilder();
        var useCase = new ListPoliciesUseCase(builder.Build());

        await useCase.ListAsync(policyholderId, PolicyStatus.Cancelled);

        builder.Mock.Verify(
            repository => repository.GetAllAsync(policyholderId, PolicyStatus.Cancelled),
            Times.Once);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoAntesDaConsulta_Quando_StatusForInvalido))]
    [Trait("Application", "Policies - Listing")]
    public async Task Deve_LancarValidacaoAntesDaConsulta_Quando_StatusForInvalido()
    {
        var builder = new PoliciesReadOnlyRepositoryBuilder();
        var useCase = new ListPoliciesUseCase(builder.Build());

        await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.ListAsync(null, (PolicyStatus)999));

        builder.Mock.Verify(
            repository => repository.GetAllAsync(It.IsAny<Guid?>(), It.IsAny<PolicyStatus?>()),
            Times.Never);
    }

    [Fact(DisplayName = nameof(Deve_MapearApoliceExistente_Quando_BuscarPorId))]
    [Trait("Application", "Policies - Listing")]
    public async Task Deve_MapearApoliceExistente_Quando_BuscarPorId()
    {
        var policy = PolicyBuilder.Build();
        var useCase = CreateUseCase(policy: policy);

        var result = await useCase.GetByIdAsync(policy.Id);

        result.Id.ShouldBe(policy.Id);
        result.Descricao.ShouldBe(policy.Description);
        result.Status.ShouldBe(policy.Status);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontrado_Quando_BuscarApoliceInexistente))]
    [Trait("Application", "Policies - Listing")]
    public async Task Deve_LancarNaoEncontrado_Quando_BuscarApoliceInexistente()
    {
        var useCase = CreateUseCase();

        await Should.ThrowAsync<NotFoundException>(() => useCase.GetByIdAsync(Guid.NewGuid()));
    }

    private static ListPoliciesUseCase CreateUseCase(IEnumerable<Policy>? policies = null, Policy? policy = null)
    {
        var readRepository = new PoliciesReadOnlyRepositoryBuilder();
        if (policies is not null)
            readRepository.GetAll(policies);
        if (policy is not null)
            readRepository.GetById(policy);
        return new ListPoliciesUseCase(readRepository.Build());
    }
}
