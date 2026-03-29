using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Interfaces;
using AutoPartsHub.Infra.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.Infra.Repositorios;

public sealed class LocalizacaoRepository(AppDbContext dbContext) : ILocalizacaoRepository
{
    public async Task<Estado?> ObterEstadoPorUfAsync(string uf, CancellationToken ct = default)
        => await dbContext.Estados
            .FirstOrDefaultAsync(e => e.Uf == uf.ToUpperInvariant(), ct);

    public async Task<Municipio?> ObterMunicipioPorNomeEUfAsync(
        string nome, int codigoUf, CancellationToken ct = default)
        => await dbContext.Municipios
            .Where(m => m.CodigoUf == codigoUf)
            .FirstOrDefaultAsync(m =>
                EF.Functions.ILike(m.Nome, nome.Trim()), ct);
}
