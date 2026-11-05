namespace PolicyDesk.Communication.Responses;

public class ResponsePolicyholdersListingJson
{
    public Guid Id { get; set; }
    public string Name { get;  set; } = string.Empty; 
    public string Cpf { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Email { get; set; } = string.Empty; 
    public string Phone {get; set; } = string.Empty; 
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}