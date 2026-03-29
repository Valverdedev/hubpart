using AutoPartsHub.Application.Common;
using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Domain.Interfaces;
using AutoPartsHub.Domain.ValueObjects;
using FluentResults;

namespace AutoPartsHub.Application.Tenants.Queries;

public sealed class ConsultarCnpjQueryHandler(
    ITenantRepository tenantRepository,
    ICnpjService cnpjService,
    ILocalizacaoService localizacaoService
) : IQueryHandler<ConsultarCnpjQuery, ConsultarCnpjResultadoDto>
{
    public async Task<Result<ConsultarCnpjResultadoDto>> Handle(
        ConsultarCnpjQuery query,
        CancellationToken ct)
    {
        // 1. Valida estrutura do CNPJ
        var cnpjResult = Cnpj.Criar(query.Cnpj);
        if (cnpjResult.IsFailed)
            return Result.Fail<ConsultarCnpjResultadoDto>(cnpjResult.Errors);

        var cnpj = cnpjResult.Value;

        // 2. Verifica duplicata na plataforma
        if (await tenantRepository.CnpjExisteAsync(cnpj.Valor, ct))
            return Result.Fail<ConsultarCnpjResultadoDto>("cnpj_ja_cadastrado");

        // 3. Consulta Receita Federal — falha silenciosa quando API indisponível
        var consultaReceita = await cnpjService.ConsultarAsync(cnpj.Valor, ct);

        if (consultaReceita is not null && !consultaReceita.Ativo)
            return Result.Fail<ConsultarCnpjResultadoDto>("cnpj_inativo");

        // 4. Resolve estado e município contra a tabela de referência interna
        ConsultarCnpjEnderecoDto? enderecoDto = null;

        if (consultaReceita is not null)
        {
            var localizacao = await localizacaoService.ResolverAsync(
                consultaReceita.Uf, consultaReceita.Cidade, ct);

            enderecoDto = new ConsultarCnpjEnderecoDto(
                Cep: consultaReceita.Cep,
                Logradouro: consultaReceita.Logradouro,
                Numero: consultaReceita.Numero,
                Complemento: consultaReceita.Complemento,
                Bairro: consultaReceita.Bairro,
                Cidade: consultaReceita.Cidade,
                Uf: consultaReceita.Uf,
                CodigoUf: localizacao.CodigoUf,
                CodigoIbge: localizacao.CodigoIbge);
        }

        var resultado = new ConsultarCnpjResultadoDto(
            Cnpj: cnpj.Formatado,
            RazaoSocial: consultaReceita?.RazaoSocial,
            NomeFantasia: consultaReceita?.NomeFantasia,
            Endereco: enderecoDto);

        return Result.Ok(resultado);
    }
}
