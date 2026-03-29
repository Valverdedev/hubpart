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
/// CodigoUf e CodigoIbge são resolvidos contra a tabela de referência interna — podem ser null
/// se estado/cidade não forem encontrados.
/// </summary>
public record ConsultarCnpjEnderecoDto(
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Uf,
    int? CodigoUf,
    int? CodigoIbge);
