using AutoPartsHub.Application.Common;
using AutoPartsHub.Application.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace AutoPartsHub.Application.Onboarding.Commands;

public sealed class ReenviarVerificacaoCommandHandler(
    IEmailService emailService,
    ICacheService cache,
    ILogger<ReenviarVerificacaoCommandHandler> logger) : ICommandHandler<ReenviarVerificacaoCommand>
{
    public async Task<Result> Handle(ReenviarVerificacaoCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result.Ok();

        var emailNormalizado = request.Email.Trim().ToLowerInvariant();
        var chave = $"reenvio:{emailNormalizado}";

        var valorAtual = await cache.ObterAsync(chave, cancellationToken);
        var tentativas = int.TryParse(valorAtual, out var quantidade) ? quantidade : 0;

        if (tentativas >= 3)
            return Result.Ok();

        var token = Guid.NewGuid().ToString("N");

        try
        {
            await emailService.EnviarVerificacaoEmailAsync(
                emailNormalizado,
                emailNormalizado,
                token,
                cancellationToken);

            await cache.SetarAsync(
                chave,
                (tentativas + 1).ToString(),
                TimeSpan.FromHours(1),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha no reenviar verificacao para {Email}", emailNormalizado);
        }

        return Result.Ok();
    }
}
