using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Interfaces;
using AutoPartsHub.Infra.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.Infra.Repositorios;

/// <summary>
/// Repositório de refresh tokens. Herda RepositorioBase para operações genéricas
/// e adiciona apenas a query específica de busca por valor do token.
/// </summary>
public sealed class RefreshTokenRepository(AppDbContext dbContext)
    : RepositorioBase<RefreshToken>(dbContext), IRefreshTokenRepository
{
    public async Task<RefreshToken?> ObterPorTokenAsync(string token, CancellationToken ct = default)
    {
        // IgnoreQueryFilters() necessário pois o token é buscado antes de saber o tenant.
        // A segurança é garantida pela verificação de TenantId no handler após obter o usuário.
        return await DbContext.RefreshTokens
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);
    }
}
