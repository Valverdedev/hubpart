namespace AutoPartsHub.Domain.Entidades;

/// <summary>
/// Registro de uso de cotações por tenant por mês.
/// PK composta: (TenantId, AnoMes).
/// NÃO herda EntidadeBase — sem tenant_id gerenciado pelo multi-tenancy.
/// Acesso controlado no handler, não via Global Query Filter.
/// </summary>
public sealed class CotacaoUsoMensal
{
    /// <summary>Identificador do tenant — parte da PK composta.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Período no formato YYYY-MM — parte da PK composta.</summary>
    public string AnoMes { get; private set; } = string.Empty;

    /// <summary>Total de cotações criadas no mês.</summary>
    public int TotalCotacoes { get; private set; }

    public DateTime AtualizadoEm { get; private set; }

    // Construtor privado para EF Core
    private CotacaoUsoMensal() { }
}
