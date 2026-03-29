using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Enums;
using AutoPartsHub.Domain.Interfaces;
using AutoPartsHub.Infra.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.Infra.Repositorios;

public sealed class TenantRepository(AppDbContext dbContext)
    : RepositorioBase<Tenant>(dbContext), ITenantRepository
{
    public async Task<bool> CnpjExisteAsync(string cnpj, CancellationToken ct = default)
        => await DbContext.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Cnpj.Valor == cnpj, ct);

    public async Task<Tenant?> ObterPorCnpjAsync(string cnpj, CancellationToken ct = default)
        => await DbContext.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Cnpj.Valor == cnpj, ct);

    public async Task<IReadOnlyList<Tenant>> ListarAguardandoAprovacaoAsync(CancellationToken ct = default)
        => await DbContext.Tenants
            .Where(t => t.Status == StatusTenant.AguardandoAprovacao)
            .ToListAsync(ct);
}
