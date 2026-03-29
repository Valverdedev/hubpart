using AutoPartsHub.Application.Common;
using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Domain.Events;
using AutoPartsHub.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoPartsHub.Application.Onboarding.Commands;

public sealed class ExpirarTrialCommandHandler(
    ITenantRepository tenantRepo,
    IPublisher publisher,
    IDateTimeProvider dateTime,
    ICacheService cache,
    ILogger<ExpirarTrialCommandHandler> logger) : ICommandHandler<ExpirarTrialCommand, ExpirarTrialResultadoDto>
{
    public async Task<Result<ExpirarTrialResultadoDto>> Handle(
        ExpirarTrialCommand request, CancellationToken cancellationToken)
    {
        // --- Query D-7 ---
        var alertasD7 = await tenantRepo.ListarTrialComExpiracaoEmAsync(7, cancellationToken);
        foreach (var tenant in alertasD7)
        {
            try
            {
                await publisher.Publish(new AlertaTrialNotification(
                    new AlertaTrialEvent(tenant.Id, tenant.Email.Valor, tenant.NomeFantasia, "D7",
                        tenant.TrialExpiraEmNovo!.Value)), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao publicar AlertaTrialEvent D7 para tenant {TenantId}", tenant.Id);
            }
        }

        // --- Query D-1 ---
        var alertasD1 = await tenantRepo.ListarTrialComExpiracaoEmAsync(1, cancellationToken);
        foreach (var tenant in alertasD1)
        {
            try
            {
                await publisher.Publish(new AlertaTrialNotification(
                    new AlertaTrialEvent(tenant.Id, tenant.Email.Valor, tenant.NomeFantasia, "D1",
                        tenant.TrialExpiraEmNovo!.Value)), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao publicar AlertaTrialEvent D1 para tenant {TenantId}", tenant.Id);
            }
        }

        // --- Query expirados ---
        var expirados = await tenantRepo.ListarTrialExpiradosAsync(cancellationToken);
        foreach (var tenant in expirados)
        {
            tenant.RebaixarParaFree(dateTime);

            // Invalidar cache Redis
            try
            {
                await cache.RemoverAsync($"tenant:{tenant.Id}", cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao invalidar cache Redis para tenant {TenantId}", tenant.Id);
            }

            try
            {
                await publisher.Publish(new TrialExpiradoNotification(
                    new TrialExpiradoEvent(tenant.Id, tenant.Email.Valor, tenant.NomeFantasia)),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao publicar TrialExpiradoEvent para tenant {TenantId}", tenant.Id);
            }
        }

        if (expirados.Count > 0)
            await tenantRepo.SalvarAlteracoesAsync(cancellationToken);

        logger.LogInformation(
            "ExpirarTrial: {Expirados} expirados, {D7} alertas D-7, {D1} alertas D-1",
            expirados.Count, alertasD7.Count, alertasD1.Count);

        return Result.Ok(new ExpirarTrialResultadoDto(expirados.Count, alertasD7.Count, alertasD1.Count));
    }
}
