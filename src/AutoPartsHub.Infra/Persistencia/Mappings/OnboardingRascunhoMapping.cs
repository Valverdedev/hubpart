using AutoPartsHub.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPartsHub.Infra.Persistencia.Mappings;

/// <summary>
/// Mapeamento da tabela onboarding_rascunho.
/// SEM Global Query Filter — tabela pública, fora do multi-tenancy.
/// </summary>
public sealed class OnboardingRascunhoMapping : IEntityTypeConfiguration<OnboardingRascunho>
{
    public void Configure(EntityTypeBuilder<OnboardingRascunho> builder)
    {
        builder.ToTable("onboarding_rascunho");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.SessionToken)
            .HasColumnName("session_token")
            .IsRequired();

        builder.HasIndex(r => r.SessionToken)
            .IsUnique()
            .HasDatabaseName("ix_onboarding_rascunho_session_token");

        builder.Property(r => r.TipoPerfil)
            .HasColumnName("tipo_perfil")
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(r => r.UltimoStep)
            .HasColumnName("ultimo_step")
            .HasDefaultValue(0);

        builder.Property(r => r.Dados)
            .HasColumnName("dados")
            .HasColumnType("jsonb")
            .HasDefaultValue("{}")
            .IsRequired();

        builder.Property(r => r.Email)
            .HasColumnName("email")
            .HasMaxLength(256);

        // Índice parcial: somente linhas onde email não é nulo
        builder.HasIndex(r => r.Email)
            .HasDatabaseName("ix_onboarding_rascunho_email")
            .HasFilter("email IS NOT NULL");

        builder.Property(r => r.IpOrigem)
            .HasColumnName("ip_origem");

        builder.Property(r => r.UserAgent)
            .HasColumnName("user_agent");

        builder.Property(r => r.CriadoEm)
            .HasColumnName("criado_em")
            .HasDefaultValueSql("now()");

        // Índice em criado_em para o job de limpeza
        builder.HasIndex(r => r.CriadoEm)
            .HasDatabaseName("ix_onboarding_rascunho_criado_em");

        builder.Property(r => r.AtualizadoEm)
            .HasColumnName("atualizado_em");
    }
}
