namespace PolicyDesk.Communication.Requests;

public class RequestClaimJson
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateOnly DataOcorrencia { get; set; }
}
