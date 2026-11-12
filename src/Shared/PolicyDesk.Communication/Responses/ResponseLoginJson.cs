namespace PolicyDesk.Communication.Responses;

public class ResponseLoginJson
{
    public string Nome { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
}
