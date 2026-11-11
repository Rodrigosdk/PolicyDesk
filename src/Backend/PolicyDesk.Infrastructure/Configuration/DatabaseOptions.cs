namespace PolicyDesk.Infrastructure.Configuration;

public sealed class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";

    public string DbConnection { get; init; } = string.Empty;
}
