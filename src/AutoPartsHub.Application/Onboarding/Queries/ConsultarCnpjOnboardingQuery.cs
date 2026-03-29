using AutoPartsHub.Application.Common;

namespace AutoPartsHub.Application.Onboarding.Queries;

public record CnpjInfoDto(
    string RazaoSocial,
    string? NomeFantasia,
    string Situacao,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado,
    string? Cep);

public record ConsultarCnpjOnboardingQuery(string Cnpj) : IQuery<CnpjInfoDto>;
