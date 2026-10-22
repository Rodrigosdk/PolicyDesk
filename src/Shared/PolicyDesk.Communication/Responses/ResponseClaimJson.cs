namespace PolicyDesk.Communication.Responses;

public class ResponseClaimJson
{
    public Guid Id { get; set; }
    public Guid ApoliceId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataOcorrencia { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? MotivoNegacao { get; set; }
    public DateTime? DataAprovacao { get; set; }
    public DateTime? DataPagamento { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
