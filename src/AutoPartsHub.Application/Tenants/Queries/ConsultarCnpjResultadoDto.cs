namespace AutoPartsHub.Application.Tenants.Queries;

/// <summary>
/// Dados retornados ao consultar um CNPJ antes do cadastro.
/// Endereço e dados cadastrais já mapeados para pré-preenchimento do formulário.
/// </summary>
public record ConsultarCnpjResultadoDto(
    string Cnpj,
    string? RazaoSocial,
    string? NomeFantasia,
    ConsultarCnpjEnderecoDto? Endereco);

/// <summary>
/// Endereço retornado pela Receita Federal, mapeado para o formato do formulário de cadastro.
/// Campos nulos indicam que a Receita Federal não retornou o dado.
/// </summary>
public record ConsultarCnpjEnderecoDto(
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro);
