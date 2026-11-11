using PolicyDesk.Communication.Enums;

namespace PolicyDesk.Communication.Responses;

public class ResponseClaimJson
{
    public Guid Id { get; set; }
    public Guid ApoliceId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateOnly DataOcorrencia { get; set; }
    public StatusSinistro Status { get; set; }
    public string? MotivoNegacao { get; set; }
    public DateTime? DataAprovacao { get; set; }
    public DateTime? DataPagamento { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}
