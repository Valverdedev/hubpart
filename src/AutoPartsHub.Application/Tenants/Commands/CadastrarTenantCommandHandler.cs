using AutoPartsHub.Application.Common;
using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Enums;
using AutoPartsHub.Domain.Interfaces;
using AutoPartsHub.Domain.ValueObjects;
using FluentResults;

namespace AutoPartsHub.Application.Tenants.Commands;

public sealed class CadastrarTenantCommandHandler(
    ITenantRepository tenantRepository,
    IDateTimeProvider dateTime
) : ICommandHandler<CadastrarTenantCommand, CadastrarTenantResultadoDto>
{
    public async Task<Result<CadastrarTenantResultadoDto>> Handle(
        CadastrarTenantCommand command,
        CancellationToken ct)
    {
        // 1. Criar Value Objects — validação estrutural
        var cnpjResult = Cnpj.Criar(command.Cnpj);
        if (cnpjResult.IsFailed)
            return Result.Fail<CadastrarTenantResultadoDto>(cnpjResult.Errors);

        // 2. Última barreira contra CNPJ duplicado (race condition entre o GET e o POST)
        //    O índice único no banco é a garantia definitiva; este check antecipa a mensagem limpa.
        if (await tenantRepository.CnpjExisteAsync(cnpjResult.Value.Valor, ct))
            return Result.Fail<CadastrarTenantResultadoDto>("cnpj_ja_cadastrado");

        var emailResult = Email.Criar(command.EmailContato);
        if (emailResult.IsFailed)
            return Result.Fail<CadastrarTenantResultadoDto>(emailResult.Errors);

        var telefones = new List<Telefone>();
        foreach (var tel in command.Telefones)
        {
            var telResult = Telefone.Criar(tel);
            if (telResult.IsFailed)
                return Result.Fail<CadastrarTenantResultadoDto>(telResult.Errors);

            telefones.Add(telResult.Value);
        }

        var endereco = Endereco.Criar(
            command.Endereco.Cep,
            command.Endereco.Logradouro,
            command.Endereco.Numero,
            command.Endereco.Complemento,
            command.Endereco.Bairro,
            command.Endereco.CodigoIbge,
            command.Endereco.CodigoUf);

        // 3. Criar o Tenant conforme o tipo
        var tipo = Enum.Parse<TipoTenant>(command.Tipo, ignoreCase: true);

        var tenant = tipo == TipoTenant.Fornecedor
            ? Tenant.CriarFornecedor(
                command.RazaoSocial,
                command.NomeFantasia,
                cnpjResult.Value,
                emailResult.Value,
                telefones,
                endereco,
                dateTime)
            : Tenant.CriarComprador(
                command.RazaoSocial,
                command.NomeFantasia,
                cnpjResult.Value,
                emailResult.Value,
                telefones,
                tipo,
                endereco,
                dateTime);

        // 4. Persistir
        await tenantRepository.AdicionarAsync(tenant, ct);
        await tenantRepository.SalvarAlteracoesAsync(ct);

        return Result.Ok(new CadastrarTenantResultadoDto(tenant.Id));
    }
}
