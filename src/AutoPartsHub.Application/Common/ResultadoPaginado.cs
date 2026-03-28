namespace AutoPartsHub.Application.Common;

/// <summary>
/// Wrapper padrão para queries de listagem paginada.
/// Toda query que retorna uma coleção deve usar este tipo como resposta.
/// </summary>
public sealed class ResultadoPaginado<T>
{
    public IReadOnlyList<T> Itens { get; }
    public int Total { get; }
    public int Pagina { get; }
    public int TamanhoPagina { get; }
    public int TotalPaginas => (int)Math.Ceiling((double)Total / TamanhoPagina);
    public bool TemProxima => Pagina < TotalPaginas;
    public bool TemAnterior => Pagina > 1;

    public ResultadoPaginado(IReadOnlyList<T> itens, int total, int pagina, int tamanhoPagina)
    {
        if (pagina < 1)
            throw new ArgumentException("Página deve ser maior que zero.", nameof(pagina));

        if (tamanhoPagina < 1)
            throw new ArgumentException("Tamanho da página deve ser maior que zero.", nameof(tamanhoPagina));

        Itens = itens;
        Total = total;
        Pagina = pagina;
        TamanhoPagina = tamanhoPagina;
    }
}
