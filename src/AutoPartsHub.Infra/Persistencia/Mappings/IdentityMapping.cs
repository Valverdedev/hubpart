using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPartsHub.Infra.Persistencia.Mappings;

/// <summary>
/// Configurações de mapeamento de todas as entidades do ASP.NET Identity.
///
/// IMPORTANTE: UseSnakeCaseNamingConvention() NÃO se aplica automaticamente quando
/// ToTable() é chamado com nome explícito nas subclasses de IdentityUser/IdentityRole.
/// Por isso, todas as colunas herdadas são mapeadas explicitamente com HasColumnName().
/// </summary>
public sealed class IdentityMapping :
    IEntityTypeConfiguration<UsuarioApp>,
    IEntityTypeConfiguration<IdentityRole<Guid>>,
    IEntityTypeConfiguration<IdentityUserRole<Guid>>,
    IEntityTypeConfiguration<IdentityUserClaim<Guid>>,
    IEntityTypeConfiguration<IdentityUserLogin<Guid>>,
    IEntityTypeConfiguration<IdentityRoleClaim<Guid>>,
    IEntityTypeConfiguration<IdentityUserToken<Guid>>,
    IEntityTypeConfiguration<RefreshToken>
{
    private readonly ITenantContext _tenantContext;

    public IdentityMapping(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public void Configure(EntityTypeBuilder<UsuarioApp> builder)
    {
        builder.ToTable("usuarios");

        // Colunas extras de UsuarioApp
        builder.Property(u => u.TenantId).HasColumnName("tenant_id");
        builder.Property(u => u.NomeCompleto).HasColumnName("nome_completo").IsRequired().HasMaxLength(200);
        builder.Property(u => u.Telefone).HasColumnName("telefone").HasMaxLength(20);
        builder.Property(u => u.LimiteAprovacao).HasColumnName("limite_aprovacao").HasPrecision(18, 2);
        builder.Property(u => u.UltimoLoginEm).HasColumnName("ultimo_login_em");

        // Colunas herdadas de IdentityUser<Guid> — mapeamento explícito necessário
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.UserName).HasColumnName("user_name").HasMaxLength(256);
        builder.Property(u => u.NormalizedUserName).HasColumnName("normalized_user_name").HasMaxLength(256);
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(256);
        builder.Property(u => u.NormalizedEmail).HasColumnName("normalized_email").HasMaxLength(256);
        builder.Property(u => u.EmailConfirmed).HasColumnName("email_confirmed");
        builder.Property(u => u.PasswordHash).HasColumnName("password_hash");
        builder.Property(u => u.SecurityStamp).HasColumnName("security_stamp");
        builder.Property(u => u.ConcurrencyStamp).HasColumnName("concurrency_stamp");
        builder.Property(u => u.PhoneNumber).HasColumnName("phone_number");
        builder.Property(u => u.PhoneNumberConfirmed).HasColumnName("phone_number_confirmed");
        builder.Property(u => u.TwoFactorEnabled).HasColumnName("two_factor_enabled");
        builder.Property(u => u.LockoutEnd).HasColumnName("lockout_end");
        builder.Property(u => u.LockoutEnabled).HasColumnName("lockout_enabled");
        builder.Property(u => u.AccessFailedCount).HasColumnName("access_failed_count");

        // Sem Global Query Filter em UsuarioApp — o UserManager precisa buscar por email
        // no login, quando ainda não há tenant_id no contexto (usuário não autenticado).
    }

    public void Configure(EntityTypeBuilder<IdentityRole<Guid>> builder)
    {
        builder.ToTable("roles");
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.Name).HasColumnName("name").HasMaxLength(256);
        builder.Property(r => r.NormalizedName).HasColumnName("normalized_name").HasMaxLength(256);
        builder.Property(r => r.ConcurrencyStamp).HasColumnName("concurrency_stamp");
    }

    public void Configure(EntityTypeBuilder<IdentityUserRole<Guid>> builder)
    {
        builder.ToTable("usuario_roles");
        builder.Property(ur => ur.UserId).HasColumnName("user_id");
        builder.Property(ur => ur.RoleId).HasColumnName("role_id");
    }

    public void Configure(EntityTypeBuilder<IdentityUserClaim<Guid>> builder)
    {
        builder.ToTable("usuario_claims");
        builder.Property(uc => uc.Id).HasColumnName("id");
        builder.Property(uc => uc.UserId).HasColumnName("user_id");
        builder.Property(uc => uc.ClaimType).HasColumnName("claim_type");
        builder.Property(uc => uc.ClaimValue).HasColumnName("claim_value");
    }

    public void Configure(EntityTypeBuilder<IdentityUserLogin<Guid>> builder)
    {
        builder.ToTable("usuario_logins");
        builder.Property(ul => ul.LoginProvider).HasColumnName("login_provider");
        builder.Property(ul => ul.ProviderKey).HasColumnName("provider_key");
        builder.Property(ul => ul.ProviderDisplayName).HasColumnName("provider_display_name");
        builder.Property(ul => ul.UserId).HasColumnName("user_id");
    }

    public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> builder)
    {
        builder.ToTable("role_claims");
        builder.Property(rc => rc.Id).HasColumnName("id");
        builder.Property(rc => rc.RoleId).HasColumnName("role_id");
        builder.Property(rc => rc.ClaimType).HasColumnName("claim_type");
        builder.Property(rc => rc.ClaimValue).HasColumnName("claim_value");
    }

    public void Configure(EntityTypeBuilder<IdentityUserToken<Guid>> builder)
    {
        builder.ToTable("usuario_tokens");
        builder.Property(ut => ut.UserId).HasColumnName("user_id");
        builder.Property(ut => ut.LoginProvider).HasColumnName("login_provider");
        builder.Property(ut => ut.Name).HasColumnName("name");
        builder.Property(ut => ut.Value).HasColumnName("value");
    }

    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        // Colunas base (id, tenant_id, criado_em, atualizado_em, excluido_em) + query filter tenant + soft-delete
        EntidadeBaseMapping.AplicarColunasPadrao(builder, _tenantContext);

        builder.Property(rt => rt.Token).HasColumnName("token").IsRequired().HasMaxLength(512);
        builder.Property(rt => rt.UsuarioId).HasColumnName("usuario_id");
        builder.Property(rt => rt.ExpiraEm).HasColumnName("expira_em");
        builder.Property(rt => rt.UsadoEm).HasColumnName("usado_em");
        builder.Property(rt => rt.Revogado).HasColumnName("revogado");

        builder.HasIndex(rt => rt.Token).IsUnique();
        builder.HasIndex(rt => new { rt.UsuarioId, rt.Revogado });

        // FK para UsuarioApp
        builder.HasOne<UsuarioApp>()
            .WithMany()
            .HasForeignKey(rt => rt.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
