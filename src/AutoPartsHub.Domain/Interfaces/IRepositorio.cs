using AutoPartsHub.Domain.Entidades;

namespace AutoPartsHub.Domain.Interfaces;

/// <summary>
/// Contrato genérico de repositório para agregados que herdam EntidadeBase.
/// Repositórios concretos herdam esta interface e adicionam apenas queries específicas.
/// </summary>
public interface IRepositorio<T> where T : EntidadeBase
{
    Task AdicionarAsync(T entidade, CancellationToken ct = default);
    Task<T?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    void Remover(T entidade);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
}
