namespace PolicyDesk.Exception.Exception;

public class NotFoundException(string message) : PolicyDeskException
{
    public override string Message { get; } = message;
}
