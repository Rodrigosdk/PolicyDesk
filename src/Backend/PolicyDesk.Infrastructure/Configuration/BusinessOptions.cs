namespace PolicyDesk.Infrastructure.Configuration;

public sealed class BusinessOptions
{
    public const string SectionName = "Business";

    public string TimeZone { get; init; } = string.Empty;
}
