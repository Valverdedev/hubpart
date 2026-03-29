using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Enums;
using AutoPartsHub.Domain.Interfaces;
using AutoPartsHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPartsHub.Infra.Persistencia.Mappings;

public sealed class TenantMapping : IEntityTypeConfiguration<Tenant>
{
    private readonly ITenantContext _tenantContext;

    public TenantMapping(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        // Colunas base (id, tenant_id, criado_em, atualizado_em, excluido_em) + query filter
        // IMPORTANTE: não chamar HasQueryFilter novamente — EntidadeBaseMapping já aplica
        EntidadeBaseMapping.AplicarColunasPadrao(builder, _tenantContext);

        builder.Property(t => t.RazaoSocial)
            .HasColumnName("razao_social")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.NomeFantasia)
            .HasColumnName("nome_fantasia")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Tipo)
            .HasColumnName("tipo")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(t => t.Plano)
            .HasColumnName("plano")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.TrialExpiraEm)
            .HasColumnName("trial_expira_em");

        builder.Property(t => t.AssinaturaRenovaEm)
            .HasColumnName("assinatura_renova_em");

        builder.Property(t => t.CotacoesUsadasNoCiclo)
            .HasColumnName("cotacoes_usadas_no_ciclo");

        builder.Property(t => t.Latitude)
            .HasColumnName("latitude");

        builder.Property(t => t.Longitude)
            .HasColumnName("longitude");

        // Value Object: Cnpj → coluna única "cnpj"
        builder.OwnsOne(t => t.Cnpj, cnpj =>
        {
            cnpj.Property(c => c.Valor)
                .HasColumnName("cnpj")
                .IsRequired()
                .HasMaxLength(14);

            // Ignorar propriedades computadas
            cnpj.Ignore(c => c.Formatado);
        });

        // Índice único global no CNPJ (não filtrado por tenant — CNPJ é único na plataforma)
        builder.HasIndex("Cnpj_Valor")
            .IsUnique()
            .HasDatabaseName("ix_tenants_cnpj");

        // Value Object: Email → coluna única "email"
        builder.OwnsOne(t => t.Email, email =>
        {
            email.Property(e => e.Valor)
                .HasColumnName("email")
                .IsRequired()
                .HasMaxLength(256);
        });

        // Value Object: Endereco → colunas com prefixo "end_"
        builder.OwnsOne(t => t.Endereco, end =>
        {
            end.Property(e => e.Cep).HasColumnName("end_cep").HasMaxLength(8);
            end.Property(e => e.Logradouro).HasColumnName("end_logradouro").HasMaxLength(200);
            end.Property(e => e.Numero).HasColumnName("end_numero").HasMaxLength(20);
            end.Property(e => e.Complemento).HasColumnName("end_complemento").HasMaxLength(100);
            end.Property(e => e.Bairro).HasColumnName("end_bairro").HasMaxLength(100);
            end.Property(e => e.CodigoIbge).HasColumnName("end_codigo_ibge");
            end.Property(e => e.CodigoUf).HasColumnName("end_codigo_uf");
        });

        // Value Object: Telefones → tabela separada "tenant_telefones"
        builder.OwnsMany(t => t.Telefones, tel =>
        {
            tel.ToTable("tenant_telefones");

            tel.WithOwner().HasForeignKey("tenant_id");

            tel.Property<int>("id")
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            tel.HasKey("id");

            tel.Property(f => f.Valor)
                .HasColumnName("valor")
                .IsRequired()
                .HasMaxLength(11);

            tel.Property(f => f.DDD)
                .HasColumnName("ddd")
                .IsRequired()
                .HasMaxLength(2);

            tel.Property(f => f.TipoTelefone)
                .HasColumnName("tipo")
                .HasConversion<string>()
                .HasMaxLength(10);

            // Propriedades computadas — não persistidas
            tel.Ignore(f => f.Numero);
            tel.Ignore(f => f.Formatado);
        });
    }
}
