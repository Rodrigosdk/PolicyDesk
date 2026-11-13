using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories.Claims;
using CommonTestUtilities.Repositories.Policies;
using PolicyDesk.Communication.Enums;
using PolicyDesk.Application.Claims.Listing;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Claims.Listing;

public class ListClaimsUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_ListarSinistrosDaApolice_Quando_ApoliceExistir))]
    [Trait("Application", "Claims - Listing")]
    public async Task Deve_ListarSinistrosDaApolice_Quando_ApoliceExistir()
    {
        var policy = PolicyBuilder.Build();
        var claims = new[]
        {
            ClaimBuilder.Build(policy.Id),
            ClaimBuilder.Build(policy.Id)
        };
        claims[1].StartAnalysis();
        var useCase = CreateUseCase(policy, claims);

        var result = (await useCase.ListByPolicyAsync(policy.Id)).ToList();

        result.Select(claim => claim.Id).ShouldBe(claims.Select(claim => claim.Id));
        result.ShouldAllBe(claim => claim.ApoliceId == policy.Id);
        result[1].Status.ShouldBe(StatusSinistro.EmAnalise);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontrado_Quando_ListarDeApoliceInexistente))]
    [Trait("Application", "Claims - Listing")]
    public async Task Deve_LancarNaoEncontrado_Quando_ListarDeApoliceInexistente()
    {
        var useCase = CreateUseCase();

        await useCase.ListByPolicyAsync(Guid.NewGuid()).ShouldThrowAsync<NotFoundException>();
    }

    [Fact(DisplayName = nameof(Deve_RetornarSinistroMapeado_Quando_BuscarPorId))]
    [Trait("Application", "Claims - Listing")]
    public async Task Deve_RetornarSinistroMapeado_Quando_BuscarPorId()
    {
        var claim = ClaimBuilder.Build(description: "Fire", amount: 2000);
        claim.StartAnalysis();
        claim.Deny("No coverage");
        var useCase = CreateUseCase(claim: claim);

        var result = await useCase.GetByIdAsync(claim.Id);

        result.Id.ShouldBe(claim.Id);
        result.ApoliceId.ShouldBe(claim.PolicyId);
        result.Descricao.ShouldBe("Fire");
        result.Valor.ShouldBe(2000);
        result.DataOcorrencia.ShouldBe(claim.OccurredOn);
        result.Status.ShouldBe(StatusSinistro.Negado);
        result.MotivoNegacao.ShouldBe("No coverage");
        result.DataAprovacao.ShouldBeNull();
        result.DataPagamento.ShouldBeNull();
        result.CriadoEm.ShouldBe(claim.CreatedAt);
        result.AtualizadoEm.ShouldBe(claim.UpdatedAt);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontrado_Quando_BuscarSinistroInexistente))]
    [Trait("Application", "Claims - Listing")]
    public async Task Deve_LancarNaoEncontrado_Quando_BuscarSinistroInexistente()
    {
        var useCase = CreateUseCase();

        await useCase.GetByIdAsync(Guid.NewGuid()).ShouldThrowAsync<NotFoundException>();
    }

    private static ListClaimsUseCase CreateUseCase(
        Policy? policy = null,
        IEnumerable<Claim>? claims = null,
        Claim? claim = null)
    {
        var claimsReadOnlyRepositoryBuilder = new ClaimsReadOnlyRepositoryBuilder();
        var policiesReadOnlyRepositoryBuilder = new PoliciesReadOnlyRepositoryBuilder();

        if (claims is not null)
            claimsReadOnlyRepositoryBuilder.GetByPolicyId(claims);

        if (claim is not null)
            claimsReadOnlyRepositoryBuilder.GetById(claim);

        if (policy is not null)
            policiesReadOnlyRepositoryBuilder.GetById(policy);

        return new ListClaimsUseCase(claimsReadOnlyRepositoryBuilder.Build(), policiesReadOnlyRepositoryBuilder.Build());
    }
}
