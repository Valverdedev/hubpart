using System.Text.Json;
using AutoPartsHub.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AutoPartsHub.Infra.Servicos;

/// <summary>
/// Consulta CNPJ na API ReceitaWS.
/// Retorna null se CNPJ inativo, inválido ou API indisponível.
/// </summary>
public sealed class ReceitaWsCnpjService(
    HttpClient httpClient,
    ILogger<ReceitaWsCnpjService> logger) : ICnpjService
{
    public async Task<CnpjConsultaResultado?> ConsultarAsync(string cnpj, CancellationToken ct = default)
    {
        try
        {
            var cnpjLimpo = new string(cnpj.Where(char.IsDigit).ToArray());
            var resposta = await httpClient.GetAsync($"cnpj/{cnpjLimpo}", ct);

            if (!resposta.IsSuccessStatusCode)
            {
                logger.LogWarning("ReceitaWS retornou {StatusCode} para CNPJ {Cnpj}", resposta.StatusCode, cnpjLimpo);
                return null;
            }

            using var stream = await resposta.Content.ReadAsStreamAsync(ct);
            var json = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            var situacao = json.RootElement.TryGetProperty("situacao", out var sit) ? sit.GetString() : null;
            var razaoSocial = json.RootElement.TryGetProperty("nome", out var rs) ? rs.GetString() ?? string.Empty : string.Empty;
            var nomeFantasia = json.RootElement.TryGetProperty("fantasia", out var nf) ? nf.GetString() : null;

            var ativo = string.Equals(situacao, "ATIVA", StringComparison.OrdinalIgnoreCase);

            string? cep = null, logradouro = null, numero = null, complemento = null, bairro = null, cidade = null, uf = null;

            if (json.RootElement.TryGetProperty("endereco", out var end))
            {
                cep = end.TryGetProperty("cep", out var c) ? c.GetString() : null;
                logradouro = end.TryGetProperty("logradouro", out var l) ? l.GetString() : null;
                numero = end.TryGetProperty("numero", out var n) ? n.GetString() : null;
                complemento = end.TryGetProperty("complemento", out var comp) ? comp.GetString() : null;
                bairro = end.TryGetProperty("bairro", out var b) ? b.GetString() : null;
                cidade = end.TryGetProperty("municipio", out var mun) ? mun.GetString() : null;
                uf = end.TryGetProperty("uf", out var u) ? u.GetString() : null;
            }

            return new CnpjConsultaResultado(ativo, razaoSocial, nomeFantasia,
                cep, logradouro, numero, complemento, bairro, cidade, uf);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao consultar CNPJ {Cnpj} na ReceitaWS", cnpj);
            return null;
        }
    }
}
