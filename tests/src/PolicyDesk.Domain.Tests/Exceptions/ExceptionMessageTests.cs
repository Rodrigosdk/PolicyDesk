using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Domain.Tests.Exceptions;

public class ExceptionMessageTests
{
    [Fact(DisplayName = nameof(Deve_ReunirTodosOsErrosNaMensagem_Quando_ValidacaoTiverVariosErros))]
    [Trait("Domain", "Exceções - Mensagem")]
    public void Deve_ReunirTodosOsErrosNaMensagem_Quando_ValidacaoTiverVariosErros()
    {
        System.Exception exception = new ErrorOnValidationException(["Nome obrigatório.", "Telefone inválido."]);

        // Lida como System.Exception, como fazem logs e ferramentas de monitoramento
        exception.Message.ShouldBe("Nome obrigatório.; Telefone inválido.");
        ((ErrorOnValidationException)exception).GetErrors().ShouldBe(["Nome obrigatório.", "Telefone inválido."]);
    }

    [Fact(DisplayName = nameof(Deve_UsarOErroComoMensagem_Quando_ValidacaoTiverUmErro))]
    [Trait("Domain", "Exceções - Mensagem")]
    public void Deve_UsarOErroComoMensagem_Quando_ValidacaoTiverUmErro()
    {
        System.Exception exception = new ErrorOnValidationException("CPF inválido.");

        exception.Message.ShouldBe("CPF inválido.");
    }

    [Fact(DisplayName = nameof(Deve_ExporAMensagem_Quando_ExcecaoForLidaComoException))]
    [Trait("Domain", "Exceções - Mensagem")]
    public void Deve_ExporAMensagem_Quando_ExcecaoForLidaComoException()
    {
        System.Exception[] exceptions =
        [
            new NotFoundException("não encontrado"),
            new ConflictException("conflito"),
            new DomainRuleException("regra violada")
        ];

        exceptions.Select(exception => exception.Message).ShouldBe(["não encontrado", "conflito", "regra violada"]);
    }
}
