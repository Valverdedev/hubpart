namespace AutoPartsHub.Domain.Interfaces;

public record CnpjConsultaResultado(
    bool Ativo,
    string RazaoSocial,
    string? NomeFantasia,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Uf);
