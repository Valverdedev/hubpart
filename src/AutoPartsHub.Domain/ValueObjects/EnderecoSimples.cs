namespace AutoPartsHub.Domain.ValueObjects;

/// <summary>
/// Value Object de endereço para o fluxo de onboarding.
/// Usa Cidade e Estado (string) em vez de CodigoIbge/CodigoUf.
/// Mapeado como OwnsOne com prefixo "endereco_" nas colunas da tabela tenants.
/// </summary>
public sealed record EnderecoSimples
{
    public string Cep { get; init; } = string.Empty;
    public string Logradouro { get; init; } = string.Empty;
    public string Numero { get; init; } = string.Empty;
    public string? Complemento { get; init; }
    public string Bairro { get; init; } = string.Empty;
    public string Cidade { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;

    private EnderecoSimples() { }

    private EnderecoSimples(
        string cep,
        string logradouro,
        string numero,
        string? complemento,
        string bairro,
        string cidade,
        string estado)
    {
        Cep = cep;
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Cidade = cidade;
        Estado = estado;
    }

    public static EnderecoSimples Criar(
        string cep,
        string logradouro,
        string numero,
        string? complemento,
        string bairro,
        string cidade,
        string estado)
        => new(cep, logradouro, numero, complemento, bairro, cidade, estado);
}
