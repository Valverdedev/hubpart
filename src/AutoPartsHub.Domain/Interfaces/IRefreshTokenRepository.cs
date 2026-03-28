using AutoPartsHub.Domain.Entidades;

namespace AutoPartsHub.Domain.Interfaces;

/// <summary>
/// Contrato para persistência e consulta de refresh tokens.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>Persiste um novo refresh token.</summary>
    Task AdicionarAsync(RefreshToken token, CancellationToken ct = default);

    /// <summary>Busca um refresh token pelo valor do token.</summary>
    Task<RefreshToken?> ObterPorTokenAsync(string token, CancellationToken ct = default);

    /// <summary>Persiste as alterações no token (usado para registrar uso/revogação).</summary>
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
}
