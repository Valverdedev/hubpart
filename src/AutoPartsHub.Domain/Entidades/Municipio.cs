namespace AutoPartsHub.Domain.Entidades;

/// <summary>
/// Representa um município brasileiro. Tabela de referência — sem herança de EntidadeBase.
/// Dados provenientes de carga externa; nunca criados pela aplicação.
/// </summary>
public sealed class Municipio
{
    public int CodigoIbge { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public bool Capital { get; private set; }
    public int CodigoUf { get; private set; }
    public string SiafiId { get; private set; } = string.Empty;
    public int Ddd { get; private set; }
    public string FusoHorario { get; private set; } = string.Empty;

    private Municipio() { }
}
