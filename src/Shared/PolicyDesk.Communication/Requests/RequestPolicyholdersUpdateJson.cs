namespace PolicyDesk.Communication.Requests;

public class RequestPolicyholdersUpdateJson
{
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; } = DateTime.MinValue;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber {get; set; } = string.Empty;
}
