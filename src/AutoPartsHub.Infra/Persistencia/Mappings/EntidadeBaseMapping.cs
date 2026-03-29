using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Infra.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPartsHub.Infra.Persistencia.Mappings;

internal static class EntidadeBaseMapping
{
    /// <summary>
    /// Aplica o mapeamento padrão das colunas de EntidadeBase e o Global Query Filter
    /// combinado (tenant + soft-delete).
    ///
    /// REGRA: não chamar HasQueryFilter novamente no mapping individual da entidade —
    /// chamadas adicionais sobrescrevem o filtro definido aqui.
    ///
    /// IMPORTANTE: recebe AppDbContext (instância scoped por request) em vez de ITenantContext
    /// para evitar que o EF Core capture o valor de TenantId no momento do build do modelo
    /// (que ocorre uma única vez e é cacheado). A lambda captura dbContext e lê TenantIdAtual
    /// em tempo de execução da query, garantindo isolamento correto por request.
    /// </summary>
    internal static void AplicarColunasPadrao<T>(
        EntityTypeBuilder<T> builder,
        AppDbContext dbContext) where T : EntidadeBase
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.TenantId).HasColumnName("tenant_id");
        builder.Property(e => e.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(e => e.AtualizadoEm).HasColumnName("atualizado_em");
        builder.Property(e => e.ExcluidoEm).HasColumnName("excluido_em");
        builder.Ignore(e => e.ExcluidoLogicamente);

        builder.HasQueryFilter(e => e.TenantId == dbContext.TenantIdAtual && e.ExcluidoEm == null);
    }
}
