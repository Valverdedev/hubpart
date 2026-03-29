using AutoPartsHub.Application.Common;
using AutoPartsHub.Application.Interfaces;
using FluentResults;

namespace AutoPartsHub.Application.Onboarding.Queries;

public sealed class ObterRascunhoQueryHandler(
    IOnboardingRascunhoRepository repositorio) : IQueryHandler<ObterRascunhoQuery, RascunhoDto>
{
    public async Task<Result<RascunhoDto>> Handle(ObterRascunhoQuery request, CancellationToken cancellationToken)
    {
        var rascunho = await repositorio.BuscarPorTokenAsync(request.SessionToken, cancellationToken);

        if (rascunho is null)
            return Result.Fail("rascunho_nao_encontrado");

        return Result.Ok(new RascunhoDto(rascunho.TipoPerfil, rascunho.UltimoStep, rascunho.Dados));
    }
}
