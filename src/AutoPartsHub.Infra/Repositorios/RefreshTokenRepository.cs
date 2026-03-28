using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Interfaces;
using AutoPartsHub.Infra.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.Infra.Repositorios;

/// <summary>
/// Implementação do repositório de refresh tokens usando EF Core.
/// O Global Query Filter do AppDbContext isola automaticamente os tokens por tenant.
/// </summary>
public sealed class RefreshTokenRepository(AppDbContext dbContext) : IRefreshTokenRepository
{
    public async Task AdicionarAsync(RefreshToken token, CancellationToken ct = default)
    {
        await dbContext.RefreshTokens.AddAsync(token, ct);
    }

    public async Task<RefreshToken?> ObterPorTokenAsync(string token, CancellationToken ct = default)
    {
        // IgnoreQueryFilters() necessário pois o token é buscado antes de saber o tenant.
        // A segurança é garantida pela verificação de TenantId no handler após obter o usuário.
        return await dbContext.RefreshTokens
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken ct = default)
    {
        await dbContext.SaveChangesAsync(ct);
    }
}
