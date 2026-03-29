using AutoPartsHub.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPartsHub.Infra.Persistencia.Mappings;

/// <summary>
/// Mapeamento da tabela cotacao_uso_mensal.
/// PK composta: (TenantId, AnoMes).
/// SEM Global Query Filter — acesso controlado no handler.
/// </summary>
public sealed class CotacaoUsoMensalMapping : IEntityTypeConfiguration<CotacaoUsoMensal>
{
    public void Configure(EntityTypeBuilder<CotacaoUsoMensal> builder)
    {
        builder.ToTable("cotacao_uso_mensal");

        // PK composta: sem auto-increment, sem sequence
        builder.HasKey(c => new { c.TenantId, c.AnoMes });

        builder.Property(c => c.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(c => c.AnoMes)
            .HasColumnName("ano_mes")
            .HasColumnType("char(7)")
            .IsRequired();

        builder.Property(c => c.TotalCotacoes)
            .HasColumnName("total_cotacoes")
            .HasDefaultValue(0);

        builder.Property(c => c.AtualizadoEm)
            .HasColumnName("atualizado_em");
    }
}
