using PolicyDesk.Communication.Enums;

namespace PolicyDesk.Communication.Responses;

public class ResponsePolicyJson
{
    public Guid Id { get; set; }
    public Guid SeguradoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorCobertura { get; set; }
    public decimal ValorPremio { get; set; }
    public DateTime InicioVigencia { get; set; }
    public DateTime FimVigencia { get; set; }
    public StatusApolice Status { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}
