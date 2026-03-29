using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Enums;

namespace AutoPartsHub.Domain.Interfaces;

public interface ITenantRepository : IRepositorio<Tenant>
{
    Task<bool> CnpjExisteAsync(string cnpj, CancellationToken ct = default);
    Task<Tenant?> ObterPorCnpjAsync(string cnpj, CancellationToken ct = default);
    Task<IReadOnlyList<Tenant>> ListarAguardandoAprovacaoAsync(CancellationToken ct = default);

    /// <summary>Tenants em trial com TrialExpiraEmNovo entre agora e agora + dias.</summary>
    Task<List<Tenant>> ListarTrialComExpiracaoEmAsync(int dias, CancellationToken ct = default);

    /// <summary>Tenants em trial cuja data de expiração já passou.</summary>
    Task<List<Tenant>> ListarTrialExpiradosAsync(CancellationToken ct = default);
}
