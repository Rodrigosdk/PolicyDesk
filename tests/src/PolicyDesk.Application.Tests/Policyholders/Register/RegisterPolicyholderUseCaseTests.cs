using CommonTestUtilities.Loggers;
using CommonTestUtilities.Repositories.Policyholders;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Time;
using PolicyDesk.Application.Policyholders.Register;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Repositories.Policyholders;
using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Policyholders.Register;

public class RegisterPolicyholderUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_RegistrarSegurado_Quando_RequisicaoForValida))]
    [Trait("Application", "Policyholders - Register")]
    public async Task Deve_RegistrarSegurado_Quando_RequisicaoForValida()
    {
        var request = RequestPolicyholderJsonBuilder.Build(email: "Jane.Doe@Example.com");
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(request);

        result.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecao_Quando_CpfForInvalido))]
    [Trait("Application", "Policyholders - Register")]
    public async Task Deve_LancarExcecao_Quando_CpfForInvalido()
    {
        var useCase = CreateUseCase();

        var exception = await useCase.ExecuteAsync(RequestPolicyholderJsonBuilder.Build(cpf: "111.111.111-11")).ShouldThrowAsync<ErrorOnValidationException>();
        exception.GetErrors().ShouldSatisfyAllConditions(errorMessage =>
        {
            errorMessage.Count.ShouldBe(1);
            errorMessage.ShouldContain(ResourcesMessageException.INVALID_CPF);
        });
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecao_Quando_EmailForInvalido))]
    [Trait("Application", "Policyholders - Register")]
    public async Task Deve_LancarExcecao_Quando_EmailForInvalido()
    {
        var useCase = CreateUseCase();

        var exception = await useCase.ExecuteAsync(RequestPolicyholderJsonBuilder.Build(email: "invalid-email")).ShouldThrowAsync<ErrorOnValidationException>();
        exception.GetErrors().ShouldSatisfyAllConditions(errorMessage =>
        {
            errorMessage.Count.ShouldBe(1);
            errorMessage.ShouldContain(ResourcesMessageException.INVALID_EMAIL);
        });
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecaoDeValidacao_Quando_JaExistirSeguradoAtivoComCpfOuEmail))]
    [Trait("Application", "Policyholders - Register")]
    [InlineData(PolicyholderRegistrationResult.ActiveCpfAlreadyExists)]
    [InlineData(PolicyholderRegistrationResult.ActiveEmailAlreadyExists)]
    public async Task Deve_LancarExcecaoDeValidacao_Quando_JaExistirSeguradoAtivoComCpfOuEmail(PolicyholderRegistrationResult result)
    {
        var useCase = CreateUseCase(result);

        var exception = await useCase.ExecuteAsync(RequestPolicyholderJsonBuilder.Build()).ShouldThrowAsync<ErrorOnValidationException>();

        var expectedError = result == PolicyholderRegistrationResult.ActiveCpfAlreadyExists
            ? ResourcesMessageException.POLICYHOLDER_ACTIVE_CPF_EXISTS
            : ResourcesMessageException.POLICYHOLDER_ACTIVE_EMAIL_EXISTS;
        exception.GetErrors().ShouldSatisfyAllConditions(errorMessage =>
        {
            errorMessage.Count.ShouldBe(1);
            errorMessage.ShouldContain(expectedError);
        });
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_NomeForVazio))]
    [Trait("Application", "Policyholders - Register")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Deve_LancarExcecao_Quando_NomeForVazio(string? name)
    {
        var request = RequestPolicyholderJsonBuilder.Build();
        request.Nome = name!;

        await ShouldFailWithError(request, ResourcesMessageException.POLICYHOLDER_NAME_REQUIRED);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecao_Quando_DataDeNascimentoNaoForInformada))]
    [Trait("Application", "Policyholders - Register")]
    public async Task Deve_LancarExcecao_Quando_DataDeNascimentoNaoForInformada()
    {
        var request = RequestPolicyholderJsonBuilder.Build();
        request.DataNascimento = default;

        await ShouldFailWithError(request, ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_REQUIRED);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecao_Quando_DataDeNascimentoForFutura))]
    [Trait("Application", "Policyholders - Register")]
    public async Task Deve_LancarExcecao_Quando_DataDeNascimentoForFutura()
    {
        var request = RequestPolicyholderJsonBuilder.Build();
        request.DataNascimento = BusinessClockBuilder.DefaultToday.AddDays(1);

        await ShouldFailWithError(request, ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_IN_FUTURE);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecao_Quando_NascimentoForAmanhaNoBrasilMasHojeEmUtc))]
    [Trait("Application", "Policyholders - Register")]
    public async Task Deve_LancarExcecao_Quando_NascimentoForAmanhaNoBrasilMasHojeEmUtc()
    {
        var request = RequestPolicyholderJsonBuilder.Build();
        request.DataNascimento = new DateOnly(2026, 3, 11);

        await ShouldFailWithError(
            request,
            ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_IN_FUTURE,
            BusinessClockBuilder.Build(BusinessClockBuilder.LateEveningUtcNow));
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_TelefoneForVazio))]
    [Trait("Application", "Policyholders - Register")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Deve_LancarExcecao_Quando_TelefoneForVazio(string? phone)
    {
        var request = RequestPolicyholderJsonBuilder.Build();
        request.Telefone = phone!;

        await ShouldFailWithError(request, ResourcesMessageException.POLICYHOLDER_PHONE_REQUIRED);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_TelefoneTiverQuantidadeDeDigitosInvalida))]
    [Trait("Application", "Policyholders - Register")]
    [InlineData("123")]
    [InlineData("(83) 8647-168")]
    [InlineData("(83) 9 8647-16871")]
    [InlineData("telefone")]
    public async Task Deve_LancarExcecao_Quando_TelefoneTiverQuantidadeDeDigitosInvalida(string phone)
    {
        var request = RequestPolicyholderJsonBuilder.Build();
        request.Telefone = phone;

        await ShouldFailWithError(request, ResourcesMessageException.INVALID_PHONE);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecao_Quando_NomeOuTelefoneExcederemOLimite))]
    [Trait("Application", "Policyholders - Register")]
    public async Task Deve_LancarExcecao_Quando_NomeOuTelefoneExcederemOLimite()
    {
        var request = RequestPolicyholderJsonBuilder.Build();
        request.Nome = new string('a', FieldLimits.PolicyholderName + 1);
        request.Telefone = "(83) 98647-1687 celular";
        var useCase = CreateUseCase();

        var exception = await useCase.ExecuteAsync(request).ShouldThrowAsync<ErrorOnValidationException>();
        exception.GetErrors().ShouldSatisfyAllConditions(errorMessage =>
        {
            errorMessage.Count.ShouldBe(2);
            errorMessage.ShouldContain(FieldLimits.PolicyholderNameTooLong);
            errorMessage.ShouldContain(ResourcesMessageException.INVALID_PHONE);
        });
    }

    [Theory(DisplayName = nameof(Deve_Registrar_Quando_TelefoneTiverDezOuOnzeDigitos))]
    [Trait("Application", "Policyholders - Register")]
    [InlineData("(83) 3333-4444")]
    [InlineData("(83) 9 8647-1687")]
    [InlineData("83986471687")]
    public async Task Deve_Registrar_Quando_TelefoneTiverDezOuOnzeDigitos(string phone)
    {
        var request = RequestPolicyholderJsonBuilder.Build();
        request.Telefone = phone;
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(request);

        result.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact(DisplayName = nameof(Deve_RetornarTodosOsErros_Quando_VariosCamposForemInvalidos))]
    [Trait("Application", "Policyholders - Register")]
    public async Task Deve_RetornarTodosOsErros_Quando_VariosCamposForemInvalidos()
    {
        var request = new RequestPolicyholderJson { Cpf = "111.111.111-11", Email = "invalid" };
        var useCase = CreateUseCase();

        var exception = await useCase.ExecuteAsync(request).ShouldThrowAsync<ErrorOnValidationException>();
        exception.GetErrors().ShouldSatisfyAllConditions(errorMessage =>
        {
            errorMessage.Count.ShouldBe(5);
            errorMessage.ShouldContain(ResourcesMessageException.INVALID_CPF);
            errorMessage.ShouldContain(ResourcesMessageException.POLICYHOLDER_NAME_REQUIRED);
            errorMessage.ShouldContain(ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_REQUIRED);
            errorMessage.ShouldContain(ResourcesMessageException.INVALID_EMAIL);
            errorMessage.ShouldContain(ResourcesMessageException.POLICYHOLDER_PHONE_REQUIRED);
        });
    }

    private static async Task ShouldFailWithError(
        RequestPolicyholderJson request,
        string expectedError,
        BusinessClock? clock = null)
    {
        var useCase = CreateUseCase(clock: clock);

        var exception = await useCase.ExecuteAsync(request).ShouldThrowAsync<ErrorOnValidationException>();
        exception.GetErrors().ShouldSatisfyAllConditions(errorMessage =>
        {
            errorMessage.Count.ShouldBe(1);
            errorMessage.ShouldContain(expectedError);
        });
    }

    private static RegisterPolicyholderUseCase CreateUseCase(
        PolicyholderRegistrationResult result = PolicyholderRegistrationResult.RegisteredOrReactivated,
        BusinessClock? clock = null)
    {
        return new RegisterPolicyholderUseCase(
            PolicyholdersWriteOnlyRepositoryBuilder.Build(result),
            clock ?? BusinessClockBuilder.Build(),
            LoggerBuilder.Build<RegisterPolicyholderUseCase>());
    }
}
