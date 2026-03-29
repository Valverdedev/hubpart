using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoPartsHub.Application.Onboarding.Commands;

/// <summary>Wrapper MediatR para publicação do CompradorCadastradoEvent na camada Application.</summary>
public sealed record CompradorCadastradoNotification(CompradorCadastradoEvent Evento) : INotification;

/// <summary>
/// Handler: envia e-mail de verificação via IEmailService.
/// Falha no e-mail é logada e não relançada — não bloqueia o cadastro.
/// </summary>
public sealed class CompradorCadastradoNotificationHandler(
    IEmailService emailService,
    ILogger<CompradorCadastradoNotificationHandler> logger) : INotificationHandler<CompradorCadastradoNotification>
{
    public async Task Handle(CompradorCadastradoNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            // Token de verificação simplificado — em produção usar UserManager.GenerateEmailConfirmationTokenAsync
            var tokenVerificacao = Guid.NewGuid().ToString("N");

            await emailService.EnviarVerificacaoEmailAsync(
                notification.Evento.Email,
                notification.Evento.NomeFantasia,
                tokenVerificacao,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao enviar e-mail de verificação para {Email}", notification.Evento.Email);
        }
    }
}
