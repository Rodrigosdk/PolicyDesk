using System.Net;
using System.Net.Http.Json;
using CommonTestUtilities.Requests;
using Shouldly;

namespace PolicyDesk.Api.IntegrationTests.Localization;

public class LocalizationTests(PolicyDeskApplicationFactory factory) : IClassFixture<PolicyDeskApplicationFactory>
{
    private const string RequestUri = "/api/segurados";

    [Theory(DisplayName = nameof(Deve_ResponderNoIdiomaPedido_Quando_AcceptLanguageForSuportado))]
    [Trait("Integration", "Localização")]
    [InlineData("en", "Invalid CPF.")]
    [InlineData("en-US", "Invalid CPF.")]
    [InlineData("pt-BR", "CPF inválido.")]
    public async Task Deve_ResponderNoIdiomaPedido_Quando_AcceptLanguageForSuportado(string language, string expectedMessage)
    {
        var body = await RegisterWithInvalidCpf(language);

        body.ShouldContain(expectedMessage);
    }

    [Theory(DisplayName = nameof(Deve_ResponderEmPortugues_Quando_AcceptLanguageNaoForSuportadoOuAusente))]
    [Trait("Integration", "Localização")]
    [InlineData(null)]
    [InlineData("fr")]
    public async Task Deve_ResponderEmPortugues_Quando_AcceptLanguageNaoForSuportadoOuAusente(string? language)
    {
        var body = await RegisterWithInvalidCpf(language);

        body.ShouldContain("CPF inválido.");
    }

    [Fact(DisplayName = nameof(Deve_FormatarMensagemComLimiteEmIngles_Quando_AcceptLanguageForIngles))]
    [Trait("Integration", "Localização")]
    public async Task Deve_FormatarMensagemComLimiteEmIngles_Quando_AcceptLanguageForIngles()
    {
        var request = RequestPolicyholderJsonBuilder.Build();
        request.Nome = new string('a', 151);

        var body = await Send(request, "en");

        body.ShouldContain("The name must have at most 150 characters.");
    }

    private async Task<string> RegisterWithInvalidCpf(string? language) =>
        await Send(RequestPolicyholderJsonBuilder.Build(cpf: "111.111.111-11"), language);

    private async Task<string> Send(object request, string? language)
    {
        using var client = factory.CreateClient();
        if (language is not null)
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(language);

        var response = await client.PostAsJsonAsync(RequestUri, request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        return await response.Content.ReadAsStringAsync();
    }
}
