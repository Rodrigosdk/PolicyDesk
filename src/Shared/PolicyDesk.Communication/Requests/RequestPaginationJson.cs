namespace PolicyDesk.Communication.Requests;

public class RequestPaginationJson
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = DefaultPageSize;
}
