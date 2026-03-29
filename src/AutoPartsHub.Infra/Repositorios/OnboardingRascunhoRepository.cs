using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Infra.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.Infra.Repositorios;

public sealed class OnboardingRascunhoRepository(AppDbContext dbContext) : IOnboardingRascunhoRepository
{
    public async Task<OnboardingRascunho?> BuscarPorTokenAsync(Guid sessionToken, CancellationToken ct)
        => await dbContext.OnboardingRascunhos
            .FirstOrDefaultAsync(r => r.SessionToken == sessionToken, ct);

    public async Task AdicionarAsync(OnboardingRascunho rascunho, CancellationToken ct)
    {
        await dbContext.OnboardingRascunhos.AddAsync(rascunho, ct);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task AtualizarAsync(OnboardingRascunho rascunho, CancellationToken ct)
    {
        dbContext.OnboardingRascunhos.Update(rascunho);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task DeletarAsync(OnboardingRascunho rascunho, CancellationToken ct)
    {
        dbContext.OnboardingRascunhos.Remove(rascunho);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<OnboardingRascunho>> BuscarAntigosAsync(DateTime antes, CancellationToken ct)
        => await dbContext.OnboardingRascunhos
            .Where(r => r.CriadoEm < antes)
            .ToListAsync(ct);
}
