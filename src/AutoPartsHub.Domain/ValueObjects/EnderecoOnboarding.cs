namespace AutoPartsHub.Domain.ValueObjects;

/// <summary>
/// Dados de endereço vindos do rascunho de onboarding.
/// Usado como parâmetro no factory method Tenant.Criar.
/// </summary>
public sealed record EnderecoOnboarding(
    string Cep,
    string Logradouro,
    string Numero,
    string? Complemento,
    string Bairro,
    string Cidade,
    string Estado);
