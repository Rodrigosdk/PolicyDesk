using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using Moq;
using PolicyDesk.Application.Policyholders.Register;
using PolicyDesk.Domain.Repositories.Policyholders;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;
using Policyholder = PolicyDesk.Domain.Entities.Policyholders;

namespace PolicyDesk.Application.Tests.Policyholders.Register;

public class RegisterPolicyholderUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_RegistrarSeguradoNormalizado_Quando_RequisicaoForValida))]
    [Trait("Application", "Policyholders - Register")]
    public async Task Deve_RegistrarSeguradoNormalizado_Quando_RequisicaoForValida()
    {
        var writeRepository = PolicyholdersWriteOnlyRepositoryBuilder.Build();
        var useCase = CreateUseCase(writeRepository);
        var request = RequestPolicyholdersJsonBuilder.Build(email: "Jane.Doe@Example.com");

        await useCase.Execute(request);

        writeRepository.Verify(
            r => r.RegisterOrReactivate(It.Is<Policyholder>(p =>
                p.Cpf == request.Cpf && p.Email == "jane.doe@example.com" && p.Name == request.Name.Trim())),
            Times.Once);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoSemChamarRepositorio_Quando_CpfForInvalido))]
    [Trait("Application", "Policyholders - Register")]
    public async Task Deve_LancarExcecaoSemChamarRepositorio_Quando_CpfForInvalido()
    {
        var writeRepository = PolicyholdersWriteOnlyRepositoryBuilder.Build();
        var useCase = CreateUseCase(writeRepository);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.Execute(RequestPolicyholdersJsonBuilder.Build(cpf: "111.111.111-11")));

        exception.GetErrors().ShouldContain(ResourcesMessageException.INVALID_CPF);
        writeRepository.Verify(r => r.RegisterOrReactivate(It.IsAny<Policyholder>()), Times.Never);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoSemChamarRepositorio_Quando_EmailForInvalido))]
    [Trait("Application", "Policyholders - Register")]
    public async Task Deve_LancarExcecaoSemChamarRepositorio_Quando_EmailForInvalido()
    {
        var writeRepository = PolicyholdersWriteOnlyRepositoryBuilder.Build();
        var useCase = CreateUseCase(writeRepository);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.Execute(RequestPolicyholdersJsonBuilder.Build(email: "invalid-email")));

        exception.GetErrors().ShouldContain(ResourcesMessageException.INVALID_EMAIL);
        writeRepository.Verify(r => r.RegisterOrReactivate(It.IsAny<Policyholder>()), Times.Never);
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
            () => useCase.Execute(RequestPolicyholdersJsonBuilder.Build()));

        var expectedError = result == PolicyholderRegistrationResult.ActiveCpfAlreadyExists
            ? ResourcesMessageException.POLICYHOLDER_ACTIVE_CPF_EXISTS
            : ResourcesMessageException.POLICYHOLDER_ACTIVE_EMAIL_EXISTS;
        exception.GetErrors().ShouldContain(expectedError);
        writeRepository.Verify(r => r.RegisterOrReactivate(It.IsAny<Policyholder>()), Times.Once);
    }

    private static RegisterPolicyholderUseCase CreateUseCase(Mock<IPolicyholdersWriteOnlyRepository> writeRepository) =>
        new(writeRepository.Object);
}
