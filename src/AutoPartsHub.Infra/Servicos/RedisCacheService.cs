using System.Text;
using AutoPartsHub.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace AutoPartsHub.Infra.Servicos;

public sealed class RedisCacheService(
    IDistributedCache cache,
    ILogger<RedisCacheService> logger) : ICacheService
{
    public async Task<bool> ExisteAsync(string chave, CancellationToken ct)
    {
        try
        {
            var valor = await cache.GetAsync(chave, ct);
            return valor is not null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao verificar chave Redis {Chave}", chave);
            return false;
        }
    }

    public async Task<string?> ObterAsync(string chave, CancellationToken ct)
    {
        try
        {
            return await cache.GetStringAsync(chave, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao obter chave Redis {Chave}", chave);
            return null;
        }
    }

    public async Task SetarAsync(string chave, string valor, TimeSpan expiracao, CancellationToken ct)
    {
        try
        {
            await cache.SetAsync(
                chave,
                Encoding.UTF8.GetBytes(valor),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiracao },
                ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao setar chave Redis {Chave}", chave);
        }
    }

    public async Task RemoverAsync(string chave, CancellationToken ct)
    {
        try
        {
            await cache.RemoveAsync(chave, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao remover chave Redis {Chave}", chave);
        }
    }
}
