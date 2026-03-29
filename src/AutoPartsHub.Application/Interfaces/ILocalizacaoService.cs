namespace AutoPartsHub.Application.Interfaces;

/// <summary>
/// Resolve UF e nome de cidade para os identificadores internos (codigo_uf, codigo_ibge).
/// Retorna null quando não encontrado — nunca bloqueia o fluxo de consulta.
/// </summary>
public interface ILocalizacaoService
{
    /// <summary>
    /// A partir da sigla UF (ex: "AM") e do nome da cidade (ex: "Manaus"),
    /// retorna os identificadores do banco de referência.
    /// Busca estado primeiro; só busca cidade se estado foi encontrado.
    /// </summary>
    Task<LocalizacaoResolvidaDto> ResolverAsync(
        string? uf, string? nomeCidade, CancellationToken ct = default);
}

/// <summary>Resultado da resolução — qualquer campo pode ser null se não encontrado.</summary>
public record LocalizacaoResolvidaDto(int? CodigoUf, int? CodigoIbge);
