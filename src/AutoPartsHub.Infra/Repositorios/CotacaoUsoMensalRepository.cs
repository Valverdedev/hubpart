using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Infra.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.Infra.Repositorios;

public sealed class CotacaoUsoMensalRepository(AppDbContext dbContext) : ICotacaoUsoMensalRepository
{
    public async Task<int> BuscarTotalAsync(Guid tenantId, string anoMes, CancellationToken ct)
    {
        var registro = await dbContext.CotacaoUsoMensal
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.AnoMes == anoMes, ct);

        return registro?.TotalCotacoes ?? 0;
    }

    public async Task IncrementarAsync(Guid tenantId, string anoMes, CancellationToken ct)
    {
        // Upsert atômico — evita race condition em concurrent requests
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO cotacao_uso_mensal (tenant_id, ano_mes, total_cotacoes, atualizado_em)
            VALUES ({tenantId}, {anoMes}, 1, now())
            ON CONFLICT (tenant_id, ano_mes)
            DO UPDATE SET
              total_cotacoes = cotacao_uso_mensal.total_cotacoes + 1,
              atualizado_em  = now()
            """, ct);
    }
}
