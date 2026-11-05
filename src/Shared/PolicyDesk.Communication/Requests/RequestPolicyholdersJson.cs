namespace PolicyDesk.Communication.Requests;

public class RequestPolicyholdersJson
{
    public string Name { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; } = DateTime.MinValue;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber {get; set; } = string.Empty;
}