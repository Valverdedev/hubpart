using AutoPartsHub.Application.Common;
using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Domain.Events;
using AutoPartsHub.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoPartsHub.Application.Onboarding.Commands;

public sealed class LimparRascunhosAntigosCommandHandler(
    IOnboardingRascunhoRepository repositorio,
    IPublisher publisher,
    IDateTimeProvider dateTime,
    ICacheService cache,
    ILogger<LimparRascunhosAntigosCommandHandler> logger) : ICommandHandler<LimparRascunhosAntigosCommand, int>
{
    public async Task<Result<int>> Handle(LimparRascunhosAntigosCommand request, CancellationToken cancellationToken)
    {
        var limite = dateTime.UtcNow.AddDays(-7);
        var antigos = await repositorio.BuscarAntigosAsync(limite, cancellationToken);

        foreach (var rascunho in antigos)
        {
            // E-mail de retomada — enviar se email preenchido e flag Redis não setada
            if (rascunho.Email is not null && rascunho.CriadoEm > limite.AddDays(-5))
            {
                var chaveRedis = $"retomada_enviada:{rascunho.SessionToken}";
                var jaEnviado = await cache.ExisteAsync(chaveRedis, cancellationToken);

                if (!jaEnviado)
                {
                    try
                    {
                        await publisher.Publish(
                            new RascunhoAbandonadoNotification(
                                new RascunhoAbandonadoEvent(rascunho.Email, rascunho.SessionToken)),
                            cancellationToken);

                        await cache.SetarAsync(chaveRedis, "1", TimeSpan.FromDays(8), cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Falha ao publicar RascunhoAbandonadoEvent para {Email}", rascunho.Email);
                    }
                }
            }

            await repositorio.DeletarAsync(rascunho, cancellationToken);
        }

        logger.LogInformation("LimparRascunhosAntigos: {Count} rascunhos removidos", antigos.Count);
        return Result.Ok(antigos.Count);
    }
}
