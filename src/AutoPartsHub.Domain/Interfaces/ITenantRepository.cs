using AutoPartsHub.Domain.Entidades;

namespace AutoPartsHub.Domain.Interfaces;

public interface ITenantRepository : IRepositorio<Tenant>
{
    Task<bool> CnpjExisteAsync(string cnpj, CancellationToken ct = default);
    Task<Tenant?> ObterPorCnpjAsync(string cnpj, CancellationToken ct = default);
    Task<IReadOnlyList<Tenant>> ListarAguardandoAprovacaoAsync(CancellationToken ct = default);
}
