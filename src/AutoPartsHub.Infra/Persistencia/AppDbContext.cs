using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Interfaces;
using AutoPartsHub.Infra.Persistencia.Mappings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.Infra.Persistencia;

/// <summary>
/// Contexto principal do banco de dados.
/// Herda IdentityDbContext para integração com ASP.NET Identity.
///
/// Regras obrigatórias:
/// - Global Query Filter por TenantId em todas as entidades multi-tenant
/// - Schema "identity" para tabelas do ASP.NET Identity
/// - EnsureCreated = false — o schema já existe no PostgreSQL
/// </summary>
public class AppDbContext : IdentityDbContext<UsuarioApp, IdentityRole<Guid>, Guid>
{
    private readonly ITenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    // --- DbSets principais ---

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Futuros agregados serão adicionados aqui:
    // public DbSet<Cotacao> Cotacoes => Set<Cotacao>();
    // public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Toda configuração de mapeamento fica em Persistencia/Mappings/
        // Instância manual necessária pois IdentityMapping recebe ITenantContext via construtor.
        // ApplyConfiguration<T> não infere o tipo quando a classe implementa múltiplas interfaces,
        // por isso cada entidade é registrada explicitamente com a mesma instância.
        var identityMapping = new IdentityMapping(_tenantContext);

        builder.ApplyConfiguration<UsuarioApp>(identityMapping);
        builder.ApplyConfiguration<IdentityRole<Guid>>(identityMapping);
        builder.ApplyConfiguration<IdentityUserRole<Guid>>(identityMapping);
        builder.ApplyConfiguration<IdentityUserClaim<Guid>>(identityMapping);
        builder.ApplyConfiguration<IdentityUserLogin<Guid>>(identityMapping);
        builder.ApplyConfiguration<IdentityRoleClaim<Guid>>(identityMapping);
        builder.ApplyConfiguration<IdentityUserToken<Guid>>(identityMapping);
        builder.ApplyConfiguration<RefreshToken>(identityMapping);
    }
}
