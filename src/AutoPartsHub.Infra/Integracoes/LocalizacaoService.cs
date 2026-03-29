using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Domain.Interfaces;

namespace AutoPartsHub.Infra.Integracoes;

public sealed class LocalizacaoService(ILocalizacaoRepository localizacaoRepository) : ILocalizacaoService
{
    public async Task<LocalizacaoResolvidaDto> ResolverAsync(
        string? uf, string? nomeCidade, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(uf))
            return new LocalizacaoResolvidaDto(null, null);

        // 1. Busca o estado pela sigla UF
        var estado = await localizacaoRepository.ObterEstadoPorUfAsync(uf, ct);
        if (estado is null)
            return new LocalizacaoResolvidaDto(null, null);

        // 2. Só busca município se tiver nome e estado resolvido
        if (string.IsNullOrWhiteSpace(nomeCidade))
            return new LocalizacaoResolvidaDto(estado.CodigoUf, null);

        var municipio = await localizacaoRepository.ObterMunicipioPorNomeEUfAsync(
            nomeCidade, estado.CodigoUf, ct);

        return new LocalizacaoResolvidaDto(estado.CodigoUf, municipio?.CodigoIbge);
    }
}
