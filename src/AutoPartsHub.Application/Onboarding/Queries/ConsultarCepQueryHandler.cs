using AutoPartsHub.Application.Common;
using AutoPartsHub.Application.Interfaces;
using FluentResults;

namespace AutoPartsHub.Application.Onboarding.Queries;

public sealed class ConsultarCepQueryHandler(
    ICepService cepService) : IQueryHandler<ConsultarCepQuery, CepInfoDto>
{
    public async Task<Result<CepInfoDto>> Handle(ConsultarCepQuery request, CancellationToken cancellationToken)
    {
        var resultado = await cepService.ConsultarAsync(request.Cep, cancellationToken);

        if (resultado is null)
            return Result.Fail("cep_nao_encontrado");

        return Result.Ok(resultado);
    }
}
