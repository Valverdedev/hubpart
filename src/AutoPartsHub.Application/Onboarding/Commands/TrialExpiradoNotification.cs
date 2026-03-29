using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoPartsHub.Application.Onboarding.Commands;

public sealed record TrialExpiradoNotification(TrialExpiradoEvent Evento) : INotification;

public sealed class TrialExpiradoNotificationHandler(
    IEmailService emailService,
    ILogger<TrialExpiradoNotificationHandler> logger) : INotificationHandler<TrialExpiradoNotification>
{
    public async Task Handle(TrialExpiradoNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await emailService.EnviarTrialExpiradoAsync(
                notification.Evento.Email,
                notification.Evento.NomeFantasia,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao enviar e-mail trial expirado para {Email}", notification.Evento.Email);
        }
    }
}
