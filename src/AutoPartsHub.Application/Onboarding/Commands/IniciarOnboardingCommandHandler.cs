using AutoPartsHub.Application.Common;
using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Interfaces;
using FluentResults;

namespace AutoPartsHub.Application.Onboarding.Commands;

public sealed class IniciarOnboardingCommandHandler(
    IOnboardingRascunhoRepository repositorio,
    IDateTimeProvider dateTime) : ICommandHandler<IniciarOnboardingCommand, Guid>
{
    public async Task<Result<Guid>> Handle(IniciarOnboardingCommand request, CancellationToken cancellationToken)
    {
        var rascunho = OnboardingRascunho.Criar(request.TipoPerfil, request.IpOrigem, request.UserAgent, dateTime);

        await repositorio.AdicionarAsync(rascunho, cancellationToken);

        return Result.Ok(rascunho.SessionToken);
    }
}
