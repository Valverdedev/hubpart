using AutoPartsHub.Application.Common;
using AutoPartsHub.Domain.Interfaces;
using FluentResults;

namespace AutoPartsHub.Application.Onboarding.Queries;

public sealed class ConsultarCnpjOnboardingQueryHandler(
    ICnpjService cnpjService) : IQueryHandler<ConsultarCnpjOnboardingQuery, CnpjInfoDto>
{
    public async Task<Result<CnpjInfoDto>> Handle(
        ConsultarCnpjOnboardingQuery request, CancellationToken cancellationToken)
    {
        var resultado = await cnpjService.ConsultarAsync(request.Cnpj, cancellationToken);

        if (resultado is null)
            return Result.Fail("cnpj_nao_encontrado");

        if (!resultado.Ativo)
            return Result.Fail("cnpj_inativo");

        return Result.Ok(new CnpjInfoDto(
            resultado.RazaoSocial,
            resultado.NomeFantasia,
            "ATIVA",
            resultado.Logradouro,
            resultado.Numero,
            resultado.Complemento,
            resultado.Bairro,
            resultado.Cidade,
            resultado.Uf,
            resultado.Cep));
    }
}
