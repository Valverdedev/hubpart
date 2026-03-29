using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoPartsHub.Application.Onboarding.Commands;

public sealed record AlertaTrialNotification(AlertaTrialEvent Evento) : INotification;

public sealed class AlertaTrialNotificationHandler(
    IEmailService emailService,
    ILogger<AlertaTrialNotificationHandler> logger) : INotificationHandler<AlertaTrialNotification>
{
    public async Task Handle(AlertaTrialNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await emailService.EnviarAlertaTrialAsync(
                notification.Evento.Email,
                notification.Evento.NomeFantasia,
                notification.Evento.Tipo,
                notification.Evento.TrialExpiraEm,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao enviar alerta trial {Tipo} para {Email}",
                notification.Evento.Tipo, notification.Evento.Email);
        }
    }
}
