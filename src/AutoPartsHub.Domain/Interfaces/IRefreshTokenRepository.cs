using AutoPartsHub.Domain.Entidades;

namespace AutoPartsHub.Domain.Interfaces;

/// <summary>
/// Contrato para persistência e consulta de refresh tokens.
/// Herda IRepositorio para operações genéricas (AdicionarAsync, SalvarAlteracoesAsync, etc.).
/// </summary>
public interface IRefreshTokenRepository : IRepositorio<RefreshToken>
{
    /// <summary>
    /// Busca um refresh token pelo hash SHA-256 do valor bruto, ignorando o Global Query Filter.
    /// Necessário pois o token chega antes de saber o tenant — a verificação de tenant
    /// é feita explicitamente no handler após obter o usuário.
    /// </summary>
    Task<RefreshToken?> ObterPorHashAsync(string tokenHash, CancellationToken ct = default);
}
