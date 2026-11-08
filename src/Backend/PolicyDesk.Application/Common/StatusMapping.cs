using PolicyDesk.Communication.Enums;
using PolicyDesk.Domain.Enums;

namespace PolicyDesk.Application.Common;

internal static class StatusMapping
{
    public static StatusApolice ToContract(this PolicyStatus status) => status switch
    {
        PolicyStatus.Active => StatusApolice.Ativa,
        PolicyStatus.Cancelled => StatusApolice.Cancelada,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    public static PolicyStatus ToDomain(this StatusApolice status) => status switch
    {
        StatusApolice.Ativa => PolicyStatus.Active,
        StatusApolice.Cancelada => PolicyStatus.Cancelled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    public static StatusSinistro ToContract(this ClaimStatus status) => status switch
    {
        ClaimStatus.Pending => StatusSinistro.Pendente,
        ClaimStatus.InAnalysis => StatusSinistro.EmAnalise,
        ClaimStatus.Approved => StatusSinistro.Aprovado,
        ClaimStatus.Denied => StatusSinistro.Negado,
        ClaimStatus.Paid => StatusSinistro.Pago,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };
}
