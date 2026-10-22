namespace PolicyDesk.Exception.Exception;

public class ErrorOnValidationException(List<string> message) : PolicyDeskException
{
    public ErrorOnValidationException(string message) : this([message])
    {
    }

    public List<string> GetErrors()
    {
        return message;
    }
}
