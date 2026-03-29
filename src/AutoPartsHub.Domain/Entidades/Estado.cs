namespace AutoPartsHub.Domain.Entidades;

/// <summary>
/// Representa um estado brasileiro. Tabela de referência — sem herança de EntidadeBase.
/// Dados provenientes de carga externa; nunca criados pela aplicação.
/// </summary>
public sealed class Estado
{
    public int CodigoUf { get; private set; }
    public string Uf { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string Regiao { get; private set; } = string.Empty;

    private Estado() { }
}
