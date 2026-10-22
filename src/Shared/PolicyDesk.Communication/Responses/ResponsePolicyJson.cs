using System.Text.Json.Serialization;
using PolicyDesk.Domain.Enums;

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
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PolicyStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
