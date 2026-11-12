using Microsoft.Extensions.Logging;

namespace PolicyDesk.Application.Logging;

// Catálogo dos eventos de log de negócio. O texto é um template fixo (não traduzir): é por ele e pelo
// EventId que os logs são agrupados e filtrados. Registre apenas IDs e valores, nunca dados pessoais.
// Faixas de EventId: 1xxx sinistros, 2xxx apólices, 3xxx segurados, 4xxx usuários.
internal static partial class ApplicationLogs
{
    // Sinistros

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Sinistro {ClaimId} registrado na apólice {PolicyId} no valor de {Amount}")]
    public static partial void ClaimRegistered(this ILogger logger, Guid claimId, Guid policyId, decimal amount);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Sinistro {ClaimId} em análise")]
    public static partial void ClaimAnalysisStarted(this ILogger logger, Guid claimId);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Information, Message = "Sinistro {ClaimId} aprovado")]
    public static partial void ClaimApproved(this ILogger logger, Guid claimId);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Information, Message = "Sinistro {ClaimId} negado")]
    public static partial void ClaimDenied(this ILogger logger, Guid claimId);

    [LoggerMessage(EventId = 1005, Level = LogLevel.Information, Message = "Pagamento do sinistro {ClaimId} registrado no valor de {Amount}")]
    public static partial void ClaimPaid(this ILogger logger, Guid claimId, decimal amount);

    // Apólices

    [LoggerMessage(EventId = 2001, Level = LogLevel.Information, Message = "Apólice {PolicyId} registrada para o segurado {PolicyholderId}")]
    public static partial void PolicyRegistered(this ILogger logger, Guid policyId, Guid policyholderId);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Information, Message = "Apólice {PolicyId} cancelada")]
    public static partial void PolicyCancelled(this ILogger logger, Guid policyId);

    // Segurados

    [LoggerMessage(EventId = 3001, Level = LogLevel.Information, Message = "Segurado cadastrado ou reativado")]
    public static partial void PolicyholderRegisteredOrReactivated(this ILogger logger);

    [LoggerMessage(EventId = 3002, Level = LogLevel.Information, Message = "Segurado {PolicyholderId} atualizado")]
    public static partial void PolicyholderUpdated(this ILogger logger, Guid policyholderId);

    [LoggerMessage(EventId = 3003, Level = LogLevel.Information, Message = "Segurado {PolicyholderId} inativado")]
    public static partial void PolicyholderDeactivated(this ILogger logger, Guid policyholderId);

    // Usuários

    [LoggerMessage(EventId = 4001, Level = LogLevel.Information, Message = "Usuário {UserId} cadastrado")]
    public static partial void UserRegistered(this ILogger logger, Guid userId);

    [LoggerMessage(EventId = 4002, Level = LogLevel.Information, Message = "Usuário {UserId} reativado")]
    public static partial void UserReactivated(this ILogger logger, Guid userId);
}
