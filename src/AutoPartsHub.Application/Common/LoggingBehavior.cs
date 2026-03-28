using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoPartsHub.Application.Common;

/// <summary>
/// Pipeline behavior que loga entrada, duração e resultado de todo command/query.
/// Registrado no MediatR antes do ValidacaoBehavior para capturar inclusive falhas de validação.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var nome = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();

        logger.LogInformation("Iniciando {Nome}", nome);

        try
        {
            var resultado = await next(ct);
            sw.Stop();
            logger.LogInformation("Concluído {Nome} em {Ms}ms", nome, sw.ElapsedMilliseconds);
            return resultado;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "Falha em {Nome} após {Ms}ms", nome, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
