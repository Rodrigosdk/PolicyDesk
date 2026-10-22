namespace PolicyDesk.Communication.Requests;

public class RequestClaimJson
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime OccurredOn { get; set; }
}
