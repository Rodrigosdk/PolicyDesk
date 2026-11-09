namespace PolicyDesk.Communication.Requests;

public class RequestPolicyJson
{
    public Guid SeguradoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorCobertura { get; set; }
    public decimal ValorPremio { get; set; }
    public DateTime InicioVigencia { get; set; }
    public DateTime FimVigencia { get; set; }
}
