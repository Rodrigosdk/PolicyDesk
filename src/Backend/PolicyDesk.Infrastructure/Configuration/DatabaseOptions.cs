namespace PolicyDesk.Infrastructure.Configuration;

public sealed class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";

    public const string MissingConnectionStringMessage =
        "ConnectionStrings:DbConnection não configurada. Informe-a via User Secrets " +
        "(dotnet user-secrets set \"ConnectionStrings:DbConnection\" \"...\") " +
        "ou pela variável de ambiente ConnectionStrings__DbConnection.";

    public string DbConnection { get; init; } = string.Empty;
}
