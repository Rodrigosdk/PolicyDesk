namespace PolicyDesk.Communication.Requests;

public class RequestUpdatePolicyholderJson
{
    public string Nome { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
}
