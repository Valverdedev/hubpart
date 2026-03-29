using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Enums;
using AutoPartsHub.Domain.ValueObjects;
using AutoPartsHub.Infra.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPartsHub.Infra.Persistencia.Mappings;

public sealed class TenantMapping : IEntityTypeConfiguration<Tenant>
{
    private readonly AppDbContext _dbContext;

    public TenantMapping(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        // Colunas base (id, tenant_id, criado_em, atualizado_em, excluido_em) + query filter
        // IMPORTANTE: não chamar HasQueryFilter novamente — EntidadeBaseMapping já aplica
        EntidadeBaseMapping.AplicarColunasPadrao(builder, _dbContext);

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

        // --- Campos de onboarding do comprador ---

        builder.Property(t => t.TipoComprador)
            .HasColumnName("tipo_comprador")
            .HasConversion<string>()
            .HasMaxLength(40);

        builder.Property(t => t.PlanoAtual)
            .HasColumnName("plano_atual")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.StatusAssinatura)
            .HasColumnName("assinatura_status")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.TrialExpiraEmNovo)
            .HasColumnName("trial_expira_em_novo");

        builder.Property(t => t.CotacoesLimiteMes)
            .HasColumnName("cotacoes_limite_mes");

        builder.Property(t => t.UsuariosLimite)
            .HasColumnName("usuarios_limite");

        builder.Property(t => t.InscricaoEstadual)
            .HasColumnName("inscricao_estadual")
            .HasMaxLength(30);

        builder.Property(t => t.TelefoneComercial)
            .HasColumnName("telefone_comercial")
            .HasMaxLength(20);

        builder.Property(t => t.ComoNosConheceu)
            .HasColumnName("como_nos_conheceu")
            .HasMaxLength(40);

        builder.Property(t => t.DescricaoOutro)
            .HasColumnName("descricao_outro");

        builder.Property(t => t.SegmentoFrota)
            .HasColumnName("segmento_frota")
            .HasMaxLength(40);

        builder.Property(t => t.QtdVeiculosEstimada)
            .HasColumnName("qtd_veiculos_estimada");

        builder.Property(t => t.LimiteAprovacaoAdmin)
            .HasColumnName("limite_aprovacao_admin")
            .HasPrecision(18, 2);

        // Value Object: EnderecoSimples → colunas com prefixo "endereco_"
        builder.OwnsOne(t => t.EnderecoSimples, end =>
        {
            end.Property(e => e.Cep).HasColumnName("endereco_cep").HasMaxLength(9);
            end.Property(e => e.Logradouro).HasColumnName("endereco_logradouro").HasMaxLength(200);
            end.Property(e => e.Numero).HasColumnName("endereco_numero").HasMaxLength(20);
            end.Property(e => e.Complemento).HasColumnName("endereco_complemento").HasMaxLength(100);
            end.Property(e => e.Bairro).HasColumnName("endereco_bairro").HasMaxLength(100);
            end.Property(e => e.Cidade).HasColumnName("endereco_cidade").HasMaxLength(100);
            end.Property(e => e.Estado).HasColumnName("endereco_estado").HasMaxLength(2);
        });

        // Value Object: Cnpj → coluna única "cnpj"
        builder.OwnsOne(t => t.Cnpj, cnpj =>
        {
            cnpj.Property(c => c.Valor)
                .HasColumnName("cnpj")
                .IsRequired()
                .HasMaxLength(14);

            // Ignorar propriedades computadas
            cnpj.Ignore(c => c.Formatado);

            // Índice único global no CNPJ (não filtrado por tenant — CNPJ é único na plataforma)
            // Definido dentro do OwnsOne pois HasIndex não suporta navegação em owned types
            cnpj.HasIndex(c => c.Valor)
                .IsUnique()
                .HasDatabaseName("ix_tenants_cnpj");
        });

        // Value Object: Email → coluna única "email"
        builder.OwnsOne(t => t.Email, email =>
        {
            email.Property(e => e.Valor)
                .HasColumnName("email")
                .IsRequired()
                .HasMaxLength(256);
        });

        // Value Object: Endereco (legado com CodigoIbge) → colunas com prefixo "end_"
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

            tel.Property(f => f.TipoTelefone)
                .HasColumnName("tipo")
                .HasConversion<string>()
                .HasMaxLength(10);

            // Propriedades computadas — derivadas de Valor, não persistidas
            tel.Ignore(f => f.DDD);
            tel.Ignore(f => f.Numero);
            tel.Ignore(f => f.Formatado);
        });
    }
}
