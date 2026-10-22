namespace PolicyDesk.Communication.Responses;

public class ResponseErrorJson
{
    public List<string> Errors { get; private set; }

    public ResponseErrorJson(List<string> errorsMessage)
    {
        Errors = errorsMessage;
    }

    public ResponseErrorJson(string errorsMessage)
    {
        Errors = [errorsMessage];
    }
}