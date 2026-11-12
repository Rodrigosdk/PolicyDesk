using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Domain.Tests.Exceptions;

public class ExceptionMessageTests
{
    [Fact(DisplayName = nameof(Deve_ReunirTodosOsErrosNaMensagem_Quando_ValidacaoTiverVariosErros))]
    [Trait("Domain", "Exceções - ErrorOnValidationException")]
    public void Deve_ReunirTodosOsErrosNaMensagem_Quando_ValidacaoTiverVariosErros()
    {
        List<string> errors = [ResourcesMessageException.POLICYHOLDER_NAME_REQUIRED, ResourcesMessageException.INVALID_PHONE];

        var exception = new ErrorOnValidationException(errors);

        exception.Message.ShouldBe($"{ResourcesMessageException.POLICYHOLDER_NAME_REQUIRED}; {ResourcesMessageException.INVALID_PHONE}");
        exception.GetErrors().ShouldBe(errors);
    }

    [Fact(DisplayName = nameof(Deve_UsarOErroComoMensagem_Quando_ValidacaoTiverUmErro))]
    [Trait("Domain", "Exceções - ErrorOnValidationException")]
    public void Deve_UsarOErroComoMensagem_Quando_ValidacaoTiverUmErro()
    {
        var exception = new ErrorOnValidationException(ResourcesMessageException.INVALID_CPF);

        exception.Message.ShouldBe(ResourcesMessageException.INVALID_CPF);
        exception.GetErrors().ShouldBe([ResourcesMessageException.INVALID_CPF]);
    }

    [Fact(DisplayName = nameof(Deve_ExporAMensagem_Quando_RecursoNaoForEncontrado))]
    [Trait("Domain", "Exceções - NotFoundException")]
    public void Deve_ExporAMensagem_Quando_RecursoNaoForEncontrado()
    {
        var exception = new NotFoundException(ResourcesMessageException.POLICYHOLDER_NOT_FOUND);

        exception.Message.ShouldBe(ResourcesMessageException.POLICYHOLDER_NOT_FOUND);
    }

    [Fact(DisplayName = nameof(Deve_ExporAMensagem_Quando_HouverConflito))]
    [Trait("Domain", "Exceções - ConflictException")]
    public void Deve_ExporAMensagem_Quando_HouverConflito()
    {
        var exception = new ConflictException(ResourcesMessageException.CONCURRENCY_CONFLICT);

        exception.Message.ShouldBe(ResourcesMessageException.CONCURRENCY_CONFLICT);
    }

    [Fact(DisplayName = nameof(Deve_ExporAMensagem_Quando_RegraDeDominioForViolada))]
    [Trait("Domain", "Exceções - DomainRuleException")]
    public void Deve_ExporAMensagem_Quando_RegraDeDominioForViolada()
    {
        var exception = new DomainRuleException(ResourcesMessageException.POLICY_ALREADY_CANCELLED);

        exception.Message.ShouldBe(ResourcesMessageException.POLICY_ALREADY_CANCELLED);
    }

    [Fact(DisplayName = nameof(Deve_ExporAMensagem_Quando_AcessoNaoForAutorizado))]
    [Trait("Domain", "Exceções - UnauthorizedException")]
    public void Deve_ExporAMensagem_Quando_AcessoNaoForAutorizado()
    {
        var exception = new UnauthorizedException(ResourcesMessageException.USER_INVALID_CREDENTIALS);

        exception.Message.ShouldBe(ResourcesMessageException.USER_INVALID_CREDENTIALS);
    }
}
