using System.Text.Json;
using AutoPartsHub.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AutoPartsHub.Infra.Servicos;

/// <summary>
/// Consulta endereço pelo CEP usando a API ViaCEP.
/// Retorna null se CEP inválido ou API indisponível.
/// </summary>
public sealed class ViaCepService(
    HttpClient httpClient,
    ILogger<ViaCepService> logger) : ICepService
{
    public async Task<CepInfoDto?> ConsultarAsync(string cep, CancellationToken ct)
    {
        try
        {
            var cepLimpo = new string(cep.Where(char.IsDigit).ToArray());
            var resposta = await httpClient.GetAsync($"ws/{cepLimpo}/json/", ct);

            if (!resposta.IsSuccessStatusCode)
            {
                logger.LogWarning("ViaCEP retornou {StatusCode} para CEP {Cep}", resposta.StatusCode, cepLimpo);
                return null;
            }

            using var stream = await resposta.Content.ReadAsStreamAsync(ct);
            var json = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            // ViaCEP retorna { "erro": "true" } para CEPs inválidos
            if (json.RootElement.TryGetProperty("erro", out _))
                return null;

            var logradouro = json.RootElement.TryGetProperty("logradouro", out var l) ? l.GetString() : null;
            var complemento = json.RootElement.TryGetProperty("complemento", out var c) ? c.GetString() : null;
            var bairro = json.RootElement.TryGetProperty("bairro", out var b) ? b.GetString() ?? string.Empty : string.Empty;
            var cidade = json.RootElement.TryGetProperty("localidade", out var loc) ? loc.GetString() ?? string.Empty : string.Empty;
            var estado = json.RootElement.TryGetProperty("uf", out var uf) ? uf.GetString() ?? string.Empty : string.Empty;
            var cepFormatado = json.RootElement.TryGetProperty("cep", out var cepProp) ? cepProp.GetString() ?? cepLimpo : cepLimpo;

            return new CepInfoDto(cepFormatado, logradouro ?? string.Empty, complemento, bairro, cidade, estado);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao consultar CEP {Cep} no ViaCEP", cep);
            return null;
        }
    }
}
