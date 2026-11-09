using PolicyDesk.Domain.Validations;
using Microsoft.Extensions.Logging.Abstractions;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using Moq;
using PolicyDesk.Application.Policyholders.Register;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Repositories.Policyholders;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;
using PolicyDesk.Domain.Entities;

namespace PolicyDesk.Application.Tests.Policyholders.Register;

public class RegisterPolicyholderUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_RegistrarSeguradoNormalizado_Quando_RequisicaoForValida))]
    [Trait("Application", "Policyholders - Register")]
    public async Task Deve_RegistrarSeguradoNormalizado_Quando_RequisicaoForValida()
    {
        var writeRepository = PolicyholdersWriteOnlyRepositoryBuilder.Build();
        var useCase = CreateUseCase(writeRepository);
        var request = RequestPolicyholderJsonBuilder.Build(email: "Jane.Doe@Example.com");

        await useCase.ExecuteAsync(request);

        writeRepository.Verify(
            r => r.RegisterOrReactivateAsync(It.Is<Policyholder>(p =>
                p.Cpf == request.Cpf && p.Email == "jane.doe@example.com" && p.Name == request.Nome.Trim())),
            Times.Once);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoSemChamarRepositorio_Quando_CpfForInvalido))]
    [Trait("Application", "Policyholders - Register")]
    public async Task Deve_LancarExcecaoSemChamarRepositorio_Quando_CpfForInvalido()
    {
        var writeRepository = PolicyholdersWriteOnlyRepositoryBuilder.Build();
        var useCase = CreateUseCase(writeRepository);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.ExecuteAsync(RequestPolicyholderJsonBuilder.Build(cpf: "111.111.111-11")));

        exception.GetErrors().ShouldContain(ResourcesMessageException.INVALID_CPF);
        writeRepository.Verify(r => r.RegisterOrReactivateAsync(It.IsAny<Policyholder>()), Times.Never);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoSemChamarRepositorio_Quando_EmailForInvalido))]
    [Trait("Application", "Policyholders - Register")]
    public async Task Deve_LancarExcecaoSemChamarRepositorio_Quando_EmailForInvalido()
    {
        var writeRepository = PolicyholdersWriteOnlyRepositoryBuilder.Build();
        var useCase = CreateUseCase(writeRepository);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.ExecuteAsync(RequestPolicyholderJsonBuilder.Build(email: "invalid-email")));

        exception.GetErrors().ShouldContain(ResourcesMessageException.INVALID_EMAIL);
        writeRepository.Verify(r => r.RegisterOrReactivateAsync(It.IsAny<Policyholder>()), Times.Never);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecaoDeValidacao_Quando_JaExistirSeguradoAtivoComCpfOuEmail))]
    [Trait("Application", "Policyholders - Register")]
    [InlineData(PolicyholderRegistrationResult.ActiveCpfAlreadyExists)]
    [InlineData(PolicyholderRegistrationResult.ActiveEmailAlreadyExists)]
    public async Task Deve_LancarExcecaoDeValidacao_Quando_JaExistirSeguradoAtivoComCpfOuEmail(PolicyholderRegistrationResult result)
    {
        var writeRepository = PolicyholdersWriteOnlyRepositoryBuilder.Build(result);
        var useCase = CreateUseCase(writeRepository);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.ExecuteAsync(RequestPolicyholderJsonBuilder.Build()));

        var expectedError = result == PolicyholderRegistrationResult.ActiveCpfAlreadyExists
            ? ResourcesMessageException.POLICYHOLDER_ACTIVE_CPF_EXISTS
            : ResourcesMessageException.POLICYHOLDER_ACTIVE_EMAIL_EXISTS;
        exception.GetErrors().ShouldContain(expectedError);
        writeRepository.Verify(r => r.RegisterOrReactivateAsync(It.IsAny<Policyholder>()), Times.Once);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecaoSemChamarRepositorio_Quando_NomeForVazio))]
    [Trait("Application", "Policyholders - Register")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Deve_LancarExcecaoSemChamarRepositorio_Quando_NomeForVazio(string? name)
    {
        var request = RequestPolicyholderJsonBuilder.Build();
        request.Nome = name!;

        await ShouldFailWithoutCallingRepository(request, ResourcesMessageException.POLICYHOLDER_NAME_REQUIRED);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoSemChamarRepositorio_Quando_DataDeNascimentoNaoForInformada))]
    [Trait("Application", "Policyholders - Register")]
    public async Task Deve_LancarExcecaoSemChamarRepositorio_Quando_DataDeNascimentoNaoForInformada()
    {
        var request = RequestPolicyholderJsonBuilder.Build();
        request.DataNascimento = default;

        await ShouldFailWithoutCallingRepository(request, ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_REQUIRED);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoSemChamarRepositorio_Quando_DataDeNascimentoForFutura))]
    [Trait("Application", "Policyholders - Register")]
    public async Task Deve_LancarExcecaoSemChamarRepositorio_Quando_DataDeNascimentoForFutura()
    {
        var request = RequestPolicyholderJsonBuilder.Build();
        request.DataNascimento = DateTime.UtcNow.Date.AddDays(1);

        await ShouldFailWithoutCallingRepository(request, ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_IN_FUTURE);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecaoSemChamarRepositorio_Quando_TelefoneForVazio))]
    [Trait("Application", "Policyholders - Register")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Deve_LancarExcecaoSemChamarRepositorio_Quando_TelefoneForVazio(string? phone)
    {
        var request = RequestPolicyholderJsonBuilder.Build();
        request.Telefone = phone!;

        await ShouldFailWithoutCallingRepository(request, ResourcesMessageException.POLICYHOLDER_PHONE_REQUIRED);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecaoSemChamarRepositorio_Quando_TelefoneTiverQuantidadeDeDigitosInvalida))]
    [Trait("Application", "Policyholders - Register")]
    [InlineData("123")]
    [InlineData("(83) 8647-168")]
    [InlineData("(83) 9 8647-16871")]
    [InlineData("telefone")]
    public async Task Deve_LancarExcecaoSemChamarRepositorio_Quando_TelefoneTiverQuantidadeDeDigitosInvalida(string phone)
    {
        var request = RequestPolicyholderJsonBuilder.Build();
        request.Telefone = phone;

        await ShouldFailWithoutCallingRepository(request, ResourcesMessageException.INVALID_PHONE);
    }

    [Theory(DisplayName = nameof(Deve_Registrar_Quando_TelefoneTiverDezOuOnzeDigitos))]
    [Trait("Application", "Policyholders - Register")]
    [InlineData("(83) 3333-4444", "+558333334444")]
    [InlineData("(83) 9 8647-1687", "+5583986471687")]
    [InlineData("83986471687", "+5583986471687")]
    public async Task Deve_Registrar_Quando_TelefoneTiverDezOuOnzeDigitos(string phone, string expectedPhone)
    {
        var writeRepository = PolicyholdersWriteOnlyRepositoryBuilder.Build();
        var useCase = CreateUseCase(writeRepository);
        var request = RequestPolicyholderJsonBuilder.Build();
        request.Telefone = phone;

        await useCase.ExecuteAsync(request);

        writeRepository.Verify(r => r.RegisterOrReactivateAsync(It.Is<Policyholder>(p => p.Phone == expectedPhone)), Times.Once);
    }

    [Fact(DisplayName = nameof(Deve_RetornarTodosOsErros_Quando_VariosCamposForemInvalidos))]
    [Trait("Application", "Policyholders - Register")]
    public async Task Deve_RetornarTodosOsErros_Quando_VariosCamposForemInvalidos()
    {
        var writeRepository = PolicyholdersWriteOnlyRepositoryBuilder.Build();
        var useCase = CreateUseCase(writeRepository);
        var request = new RequestPolicyholderJson { Cpf = "111.111.111-11", Email = "invalid" };

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(() => useCase.ExecuteAsync(request));

        exception.GetErrors().ShouldBe(
        [
            ResourcesMessageException.INVALID_CPF,
            ResourcesMessageException.POLICYHOLDER_NAME_REQUIRED,
            ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_REQUIRED,
            ResourcesMessageException.INVALID_EMAIL,
            ResourcesMessageException.POLICYHOLDER_PHONE_REQUIRED
        ], ignoreOrder: true);
        writeRepository.Verify(r => r.RegisterOrReactivateAsync(It.IsAny<Policyholder>()), Times.Never);
    }

    private static async Task ShouldFailWithoutCallingRepository(RequestPolicyholderJson request, string expectedError)
    {
        var writeRepository = PolicyholdersWriteOnlyRepositoryBuilder.Build();
        var useCase = CreateUseCase(writeRepository);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(() => useCase.ExecuteAsync(request));

        exception.GetErrors().ShouldBe([expectedError]);
        writeRepository.Verify(r => r.RegisterOrReactivateAsync(It.IsAny<Policyholder>()), Times.Never);
    }

    private static RegisterPolicyholderUseCase CreateUseCase(Mock<IPolicyholdersWriteOnlyRepository> writeRepository) =>
        new(writeRepository.Object, NullLogger<RegisterPolicyholderUseCase>.Instance);


    [Fact(DisplayName = nameof(Deve_LancarExcecaoSemChamarRepositorio_Quando_NomeOuTelefoneExcederemOLimite))]
    [Trait("Application", "Policyholders - Register")]
    public async Task Deve_LancarExcecaoSemChamarRepositorio_Quando_NomeOuTelefoneExcederemOLimite()
    {
        var writeRepository = PolicyholdersWriteOnlyRepositoryBuilder.Build();
        var useCase = CreateUseCase(writeRepository);
        var request = RequestPolicyholderJsonBuilder.Build();
        request.Nome = new string('a', FieldLimits.PolicyholderName + 1);
        request.Telefone = "(83) 98647-1687 celular";

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(() => useCase.ExecuteAsync(request));

        exception.GetErrors().ShouldBe([FieldLimits.PolicyholderNameTooLong, ResourcesMessageException.INVALID_PHONE]);
        writeRepository.Verify(r => r.RegisterOrReactivateAsync(It.IsAny<Policyholder>()), Times.Never);
    }
}
