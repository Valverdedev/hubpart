using AutoPartsHub.Domain.Entidades;

namespace AutoPartsHub.Domain.Interfaces;

/// <summary>
/// Consultas às tabelas de referência de estados e municípios.
/// Sem Global Query Filter — dados públicos, sem isolamento por tenant.
/// </summary>
public interface ILocalizacaoRepository
{
    Task<Estado?> ObterEstadoPorUfAsync(string uf, CancellationToken ct = default);
    Task<Municipio?> ObterMunicipioPorNomeEUfAsync(string nome, int codigoUf, CancellationToken ct = default);
}
