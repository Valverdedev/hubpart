namespace AutoPartsHub.Domain.ValueObjects;

/// <summary>
/// Value Object que representa um endereço completo.
/// CodigoIbge referencia a tabela "municipios" (PK: codigo_ibge).
/// CodigoUf referencia a tabela "estados" (PK: codigo_uf).
/// Mapeado como OwnsOne no EF Core com prefixo "end_" nas colunas.
/// </summary>
public sealed record Endereco
{
    public string Cep { get; init; }
    public string Logradouro { get; init; }
    public string Numero { get; init; }
    public string? Complemento { get; init; }
    public string Bairro { get; init; }
    public int CodigoIbge { get; init; }
    public int CodigoUf { get; init; }

    private Endereco(
        string cep,
        string logradouro,
        string numero,
        string? complemento,
        string bairro,
        int codigoIbge,
        int codigoUf)
    {
        Cep = cep;
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        CodigoIbge = codigoIbge;
        CodigoUf = codigoUf;
    }

    public static Endereco Criar(
        string cep,
        string logradouro,
        string numero,
        string? complemento,
        string bairro,
        int codigoIbge,
        int codigoUf)
    {
        return new Endereco(cep, logradouro, numero, complemento, bairro, codigoIbge, codigoUf);
    }
}
