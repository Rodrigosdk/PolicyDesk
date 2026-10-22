using System.Net;
using Shouldly;

namespace PolicyDesk.Api.IntegrationTests.Policyholders.Deactivate;

public class DeactivatePolicyholderTests(PolicyDeskApplicationFactory factory) : PolicyholdersTestBase(factory)
{
    [Fact(DisplayName = nameof(Deve_Inativar_Quando_NaoHouverApoliceAtiva))]
    [Trait("Integration", "Policyholders - Inativação")]
    public async Task Deve_Inativar_Quando_NaoHouverApoliceAtiva()
    {
        var policyholder = await RegisterPolicyholder();

        var response = await Delete($"{RequestUri}/{policyholder.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await FindById(policyholder.Id)).Active.ShouldBeFalse();
        (await Get($"{RequestUri}/{policyholder.Id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_HouverApoliceAtiva))]
    [Trait("Integration", "Policyholders - Inativação")]
    public async Task Deve_RetornarBadRequest_Quando_HouverApoliceAtiva()
    {
        var policyholder = await RegisterPolicyholder();
        (await RegisterPolicy(policyholder.Id)).StatusCode.ShouldBe(HttpStatusCode.Created);

        var response = await Delete($"{RequestUri}/{policyholder.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await FindById(policyholder.Id)).Active.ShouldBeTrue();
    }

    [Fact(DisplayName = nameof(Deve_RetornarNotFound_Quando_SeguradoNaoExistir))]
    [Trait("Integration", "Policyholders - Inativação")]
    public async Task Deve_RetornarNotFound_Quando_SeguradoNaoExistir()
    {
        var response = await Delete($"{RequestUri}/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
