using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoPartsHub.Application.Onboarding.Commands;

public sealed record RascunhoAbandonadoNotification(RascunhoAbandonadoEvent Evento) : INotification;

public sealed class RascunhoAbandonadoNotificationHandler(
    IEmailService emailService,
    ILogger<RascunhoAbandonadoNotificationHandler> logger) : INotificationHandler<RascunhoAbandonadoNotification>
{
    public async Task Handle(RascunhoAbandonadoNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await emailService.EnviarRetomadaCadastroAsync(
                notification.Evento.Email,
                notification.Evento.SessionToken.ToString(),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao enviar e-mail de retomada para {Email}", notification.Evento.Email);
        }
    }
}
