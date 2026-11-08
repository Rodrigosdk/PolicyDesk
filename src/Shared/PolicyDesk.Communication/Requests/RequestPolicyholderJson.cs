namespace PolicyDesk.Communication.Requests;

public class RequestPolicyholderJson
{
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
}
