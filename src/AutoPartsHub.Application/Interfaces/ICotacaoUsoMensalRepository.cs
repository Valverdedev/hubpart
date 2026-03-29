namespace AutoPartsHub.Application.Interfaces;

public interface ICotacaoUsoMensalRepository
{
    Task<int> BuscarTotalAsync(Guid tenantId, string anoMes, CancellationToken ct);
    Task IncrementarAsync(Guid tenantId, string anoMes, CancellationToken ct);
}
