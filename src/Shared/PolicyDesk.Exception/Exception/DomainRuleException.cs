namespace PolicyDesk.Exception.Exception;

// Uma regra de negócio do domínio foi violada (dado inválido ou operação não permitida no estado atual)
public class DomainRuleException(string message) : PolicyDeskException
{
    public override string Message { get; } = message;
}
