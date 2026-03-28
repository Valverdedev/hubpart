using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Interfaces;
using AutoPartsHub.Infra.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.Infra.Repositorios;

/// <summary>
/// Implementação base de repositório para agregados que herdam EntidadeBase.
/// Repositórios concretos herdam esta classe e adicionam apenas queries específicas.
/// O Global Query Filter do AppDbContext garante isolamento por tenant automaticamente.
/// </summary>
public abstract class RepositorioBase<T>(AppDbContext dbContext) : IRepositorio<T>
    where T : EntidadeBase
{
    protected readonly AppDbContext DbContext = dbContext;

    public async Task AdicionarAsync(T entidade, CancellationToken ct = default)
        => await DbContext.Set<T>().AddAsync(entidade, ct);

    public async Task<T?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => await DbContext.Set<T>().FirstOrDefaultAsync(e => e.Id == id, ct);

    public void Remover(T entidade)
        => DbContext.Set<T>().Remove(entidade);

    public async Task SalvarAlteracoesAsync(CancellationToken ct = default)
        => await DbContext.SaveChangesAsync(ct);
}
