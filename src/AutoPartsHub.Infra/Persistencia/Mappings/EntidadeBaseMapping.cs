using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Interfaces;
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
    /// </summary>
    internal static void AplicarColunasPadrao<T>(
        EntityTypeBuilder<T> builder,
        ITenantContext tenantContext) where T : EntidadeBase
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.TenantId).HasColumnName("tenant_id");
        builder.Property(e => e.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(e => e.AtualizadoEm).HasColumnName("atualizado_em");
        builder.Property(e => e.ExcluidoEm).HasColumnName("excluido_em");
        builder.Ignore(e => e.ExcluidoLogicamente);

        builder.HasQueryFilter(e => e.TenantId == tenantContext.TenantId && e.ExcluidoEm == null);
    }
}
