namespace AutoPartsHub.Application.Interfaces;

/// <summary>Abstrai operações de cache distribuído (Redis) para a camada Application.</summary>
public interface ICacheService
{
    Task<bool> ExisteAsync(string chave, CancellationToken ct);
    Task<string?> ObterAsync(string chave, CancellationToken ct);
    Task SetarAsync(string chave, string valor, TimeSpan expiracao, CancellationToken ct);
    Task RemoverAsync(string chave, CancellationToken ct);
}
