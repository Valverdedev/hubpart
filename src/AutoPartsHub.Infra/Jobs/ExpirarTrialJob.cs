using AutoPartsHub.Application.Onboarding.Commands;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoPartsHub.Infra.Jobs;

public sealed class ExpirarTrialJob(ISender sender, ILogger<ExpirarTrialJob> logger)
{
    [JobDisplayName("Expirar trial e alertas")]
    public async Task ExecutarAsync()
    {
        var resultado = await sender.Send(new ExpirarTrialCommand());

        if (resultado.IsSuccess)
        {
            logger.LogInformation(
                "ExpirarTrialJob: {Expirados} expirados, {D7} alertas D-7, {D1} alertas D-1",
                resultado.Value.Expirados, resultado.Value.AlertasD7, resultado.Value.AlertasD1);
        }
        else
        {
            logger.LogError("ExpirarTrialJob falhou: {Erros}", string.Join(", ", resultado.Errors));
        }

        await sender.Send(new LimparRascunhosAntigosCommand());
    }
}
