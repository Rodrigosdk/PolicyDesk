namespace PolicyDesk.Communication.Requests;

public class RequestPolicyJson
{
    public Guid PolicyholderId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal CoverageAmount { get; set; }
    public decimal PremiumAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
