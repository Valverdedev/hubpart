namespace AutoPartsHub.Application.Tenants.Commands;

public record EnderecoInputDto(
    string Cep,
    string Logradouro,
    string Numero,
    string? Complemento,
    string Bairro,
    int CodigoIbge,
    int CodigoUf);
