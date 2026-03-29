using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Interfaces;
using AutoPartsHub.Infra.Identity;
using AutoPartsHub.Infra.Persistencia.Mappings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
    private readonly IPublisher _publisher;
    private readonly IDateTimeProvider _dateTime;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ITenantContext tenantContext,
        IPublisher publisher,
        IDateTimeProvider dateTime)
        : base(options)
    {
        _tenantContext = tenantContext;
        _publisher = publisher;
        _dateTime = dateTime;
    }

    /// <summary>
    /// Expõe o TenantId da request atual para uso nos Global Query Filters.
    /// A lambda do HasQueryFilter captura esta instância (scoped por request),
    /// evitando que o EF Core cache o valor do primeiro request para todos os demais.
    /// </summary>
    public Guid TenantIdAtual => _tenantContext.TenantId;

    // --- DbSets principais ---

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Estado> Estados => Set<Estado>();
    public DbSet<Municipio> Municipios => Set<Municipio>();

    // Futuros agregados serão adicionados aqui:
    // public DbSet<Cotacao> Cotacoes => Set<Cotacao>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Toda configuração de mapeamento fica em Persistencia/Mappings/
        // Instância manual necessária pois IdentityMapping recebe ITenantContext via construtor.
        // ApplyConfiguration<T> não infere o tipo quando a classe implementa múltiplas interfaces,
        // por isso cada entidade é registrada explicitamente com a mesma instância.
        var identityMapping = new IdentityMapping(this);
        var tenantMapping = new TenantMapping(this);

        builder.ApplyConfiguration<Estado>(new EstadoMapping());
        builder.ApplyConfiguration<Municipio>(new MunicipioMapping());
        builder.ApplyConfiguration<Tenant>(tenantMapping);
        builder.ApplyConfiguration<UsuarioApp>(identityMapping);
        builder.ApplyConfiguration<IdentityRole<Guid>>(identityMapping);
        builder.ApplyConfiguration<IdentityUserRole<Guid>>(identityMapping);
        builder.ApplyConfiguration<IdentityUserClaim<Guid>>(identityMapping);
        builder.ApplyConfiguration<IdentityUserLogin<Guid>>(identityMapping);
        builder.ApplyConfiguration<IdentityRoleClaim<Guid>>(identityMapping);
        builder.ApplyConfiguration<IdentityUserToken<Guid>>(identityMapping);
        builder.ApplyConfiguration<RefreshToken>(identityMapping);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AtualizarTimestamps();

        // Coleta eventos antes de salvar — após SaveChanges as entidades podem ser desanexadas
        var eventos = ColetarEventos();

        var resultado = await base.SaveChangesAsync(cancellationToken);

        await PublicarEventosAsync(eventos, cancellationToken);

        return resultado;
    }

    private void AtualizarTimestamps()
    {
        var entidadesModificadas = ChangeTracker.Entries<EntidadeBase>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entidadesModificadas)
            entry.Entity.MarcarComoAtualizado(_dateTime);
    }

    private List<IDomainEvent> ColetarEventos()
    {
        var eventos = ChangeTracker.Entries<EntidadeBase>()
            .SelectMany(e => e.Entity.Eventos)
            .ToList();

        foreach (var entry in ChangeTracker.Entries<EntidadeBase>())
            entry.Entity.LimparEventos();

        return eventos;
    }

    private async Task PublicarEventosAsync(List<IDomainEvent> eventos, CancellationToken ct)
    {
        foreach (var evento in eventos)
            await _publisher.Publish(new DomainEventAdapter(evento), ct);
    }
}
