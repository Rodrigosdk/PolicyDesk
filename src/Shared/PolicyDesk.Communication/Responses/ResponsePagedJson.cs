namespace PolicyDesk.Communication.Responses;

public class ResponsePagedJson<T>
{
    public IReadOnlyList<T> Itens { get; set; } = [];
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalItens { get; set; }
    public int TotalPaginas { get; set; }
}
