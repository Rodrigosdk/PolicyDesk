namespace PolicyDesk.Exception.Exception;

// O registro foi alterado por outra operação entre a leitura e a gravação
public class ConflictException(string message) : PolicyDeskException
{
    public override string Message { get; } = message;
}
