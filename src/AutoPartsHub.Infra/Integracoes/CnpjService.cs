using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AutoPartsHub.Domain.Interfaces;

namespace AutoPartsHub.Infra.Integracoes;

/// <summary>
/// Implementação real do ICnpjService consumindo a API pública open.cnpja.com.
/// Em caso de falha (timeout, CNPJ não encontrado, API indisponível) retorna null —
/// o handler trata null como prosseguir sem dados pré-preenchidos.
/// </summary>
public sealed class CnpjService(HttpClient httpClient) : ICnpjService
{
    public async Task<CnpjConsultaResultado?> ConsultarAsync(string cnpj, CancellationToken ct = default)
    {
        try
        {
            var resposta = await httpClient.GetFromJsonAsync<CnpjaResponse>(
                $"office/{cnpj}", ct);

            if (resposta is null)
                return null;

            return new CnpjConsultaResultado(
                Ativo: resposta.Status?.Id == 2,
                RazaoSocial: resposta.Company?.Name ?? string.Empty,
                NomeFantasia: resposta.Alias,
                Cep: resposta.Address?.Zip,
                Logradouro: resposta.Address?.Street,
                Numero: resposta.Address?.Number,
                Complemento: resposta.Address?.Details,
                Bairro: resposta.Address?.District,
                Cidade: resposta.Address?.City,
                Uf: resposta.Address?.State);
        }
        catch
        {
            // Falha silenciosa — API indisponível não impede o cadastro
            return null;
        }
    }

    // ---------------------------------------------------------------------------
    // DTOs internos para desserialização da resposta do open.cnpja.com
    // ---------------------------------------------------------------------------

    private sealed record CnpjaResponse(
        [property: JsonPropertyName("alias")]   string? Alias,
        [property: JsonPropertyName("company")] CnpjaCompany? Company,
        [property: JsonPropertyName("status")]  CnpjaStatus? Status,
        [property: JsonPropertyName("address")] CnpjaAddress? Address);

    private sealed record CnpjaCompany(
        [property: JsonPropertyName("name")] string? Name);

    private sealed record CnpjaStatus(
        [property: JsonPropertyName("id")]   int Id,
        [property: JsonPropertyName("text")] string? Text);

    private sealed record CnpjaAddress(
        [property: JsonPropertyName("zip")]      string? Zip,
        [property: JsonPropertyName("street")]   string? Street,
        [property: JsonPropertyName("number")]   string? Number,
        [property: JsonPropertyName("details")]  string? Details,
        [property: JsonPropertyName("district")] string? District,
        [property: JsonPropertyName("city")]     string? City,
        [property: JsonPropertyName("state")]    string? State);
}
