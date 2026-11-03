namespace PolicyDesk.Api.Logging;

// Catálogo dos eventos de log da API. O texto é um template fixo (não traduzir): é por ele e pelo
// EventId que os logs são agrupados e filtrados. Faixa de EventId: 9xxx.
internal static partial class ApiLogs
{
    [LoggerMessage(EventId = 9001, Level = LogLevel.Error, Message = "Erro não tratado em {Method} {Path}")]
    public static partial void UnhandledException(this ILogger logger, System.Exception exception, string method, string path);
}
