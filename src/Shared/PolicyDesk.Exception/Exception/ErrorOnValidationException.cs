namespace PolicyDesk.Exception.Exception;

public class ErrorOnValidationException(List<string> errors) : PolicyDeskException(string.Join("; ", errors))
{
    public ErrorOnValidationException(string error) : this([error])
    {
    }

    public List<string> GetErrors()
    {
        return errors;
    }
}
