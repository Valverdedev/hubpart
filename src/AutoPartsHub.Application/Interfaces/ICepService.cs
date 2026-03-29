namespace AutoPartsHub.Application.Interfaces;

public record CepInfoDto(
    string Cep,
    string Logradouro,
    string? Complemento,
    string Bairro,
    string Cidade,
    string Estado);

public interface ICepService
{
    Task<CepInfoDto?> ConsultarAsync(string cep, CancellationToken ct);
}
