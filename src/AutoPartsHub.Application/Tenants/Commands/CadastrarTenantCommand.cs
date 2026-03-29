using AutoPartsHub.Application.Common;

namespace AutoPartsHub.Application.Tenants.Commands;

public record CadastrarTenantCommand(
    string RazaoSocial,
    string NomeFantasia,
    string Cnpj,
    string Tipo,
    string EmailContato,
    List<string> Telefones,
    EnderecoInputDto Endereco) : ICommand<CadastrarTenantResultadoDto>;
